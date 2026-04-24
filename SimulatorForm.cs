using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using smart_attendance_middleware.Classes;

namespace smart_attendance_middleware
{
    public partial class SimulatorForm : Form
    {
        private readonly DatabaseHelper _db;
        private readonly WebSocketManager _ws;

        private CancellationTokenSource _cts;
        private AttendanceSimulator _sim;
        private DateTime? _untilUtc;

        // UI-bound lists (kept small)
        private readonly BindingList<EmitRow> _emits = new BindingList<EmitRow>();
        private readonly BindingList<PairRow> _pairs = new BindingList<PairRow>();

        // Ingestion queues (fast, lock-free)
        private readonly ConcurrentQueue<EmitRow> _emitQueue = new ConcurrentQueue<EmitRow>();
        private readonly ConcurrentQueue<PairRow> _pairQueue = new ConcurrentQueue<PairRow>();

        // Flush timer to batch UI updates
        private readonly System.Windows.Forms.Timer _flushTimer = new System.Windows.Forms.Timer();

        // Limits and batch size to prevent UI stalls
        private const int MaxRows = 500;         // Increased limit to prevent losing data visually
        private const int FlushBatchSize = 250;  // max rows to add per tick
        private const int FlushIntervalMs = 200; // batching cadence

        public SimulatorForm(DatabaseHelper db, WebSocketManager ws) : this()
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _ws = ws ?? throw new ArgumentNullException(nameof(ws));
        }

        public SimulatorForm()
        {
            InitializeComponent();
        }

        private void SimulatorForm_Load(object sender, EventArgs e)
        {
            // Prepare grids (avoid AutoGenerateColumns for perf; let WinForms create columns once)
            gridEmits.AutoGenerateColumns = true;
            gridPairs.AutoGenerateColumns = true;
            gridEmits.DataSource = _emits;
            gridPairs.DataSource = _pairs;

            // Improve grid responsiveness
            gridEmits.VirtualMode = false; // keep binding simple
            gridEmits.ReadOnly = true;
            gridPairs.VirtualMode = false;
            gridPairs.ReadOnly = true;
            gridEmits.RowHeadersVisible = false;
            gridPairs.RowHeadersVisible = false;
            gridEmits.AllowUserToAddRows = false;
            gridEmits.AllowUserToDeleteRows = false;
            gridPairs.AllowUserToAddRows = false;
            gridPairs.AllowUserToDeleteRows = false;

            // Subscribe simulator events (producer side only queues; UI flushes in batches)
            SimulationEvents.SimulationStarted += OnSimulationStarted;
            SimulationEvents.SimulationStopped += OnSimulationStopped;
            SimulationEvents.Scenario += OnScenario;
            SimulationEvents.Emitted += OnEmitted;
            SimulationEvents.Paired += OnPaired;

            // Remaining time label
            timer1.Interval = 500;
            timer1.Tick += Timer1_Tick;
            timer1.Start();

            // Batch flush timer
            _flushTimer.Interval = FlushIntervalMs;
            _flushTimer.Tick += FlushTimer_Tick;
            _flushTimer.Start();

            UpdateUiState();
        }

        private void SimulatorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _flushTimer.Stop();
                _flushTimer.Tick -= FlushTimer_Tick;

                timer1.Stop();
                timer1.Tick -= Timer1_Tick;

                SimulationEvents.SimulationStarted -= OnSimulationStarted;
                SimulationEvents.SimulationStopped -= OnSimulationStopped;
                SimulationEvents.Scenario -= OnScenario;
                SimulationEvents.Emitted -= OnEmitted;
                SimulationEvents.Paired -= OnPaired;

                _cts?.Cancel();
            }
            catch { /* ignore */ }

            _cts?.Dispose();
            _cts = null;
            _sim = null;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_cts != null) return;
            if (_db == null || _ws == null)
            {
                MessageBox.Show(this, "Simulator requires DatabaseHelper and WebSocketManager.", "Missing dependencies", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Clear UI lists (fast) and queues
            _emits.Clear();
            _pairs.Clear();
            while (_emitQueue.TryDequeue(out _)) { }
            while (_pairQueue.TryDequeue(out _)) { }
            txtLog.Clear();

            _cts = new CancellationTokenSource();
            int minutes = (int)numHours.Value;
            _sim = new AttendanceSimulator(_db, _ws, minutes);

            // Run simulator background
            Task.Run(() => _sim.RunAsync(_cts.Token));
            AppendLog($"Simulation requested to start for {minutes} minute(s).");
            lblStatus.Text = "Running";
            UpdateUiState();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try { _cts?.Cancel(); } catch { /* ignore */ }
            AppendLog("Stop requested.");
            UpdateUiState();
        }

        private void OnSimulationStarted(object sender, SimulationEvents.SimulationStartedEventArgs e)
        {
            // Lightweight UI update
            if (IsHandleCreated)
            {
                BeginInvoke((Action)(() =>
                {
                    _untilUtc = e.UntilUtc;
                    lblStatus.Text = "Running";
                    AppendLog($"Simulation started. Auto-stop at {e.UntilUtc.ToLocalTime():HH:mm:ss}.");
                    UpdateUiState();
                }));
            }
        }

        private void OnSimulationStopped(object sender, SimulationEvents.SimulationStoppedEventArgs e)
        {
            if (IsHandleCreated)
            {
                BeginInvoke((Action)(() =>
                {
                    AppendLog($"Simulation stopped: {e.Reason}");
                    _untilUtc = null;
                    _cts?.Dispose();
                    _cts = null;
                    _sim = null;
                    lblStatus.Text = "Stopped";
                    UpdateUiState();
                }));
            }
        }

        private void OnScenario(object sender, SimulationEvents.ScenarioEventArgs e)
        {
            // Avoid heavy UI work per event
            if (IsHandleCreated)
            {
                BeginInvoke((Action)(() =>
                {
                    AppendLog($"Scenario: {e.ScenarioName} for {e.MemberName} (ID={e.MemberId}).");
                }));
            }
        }

        // Producer: enqueue only
        private void OnEmitted(object sender, SimulationEvents.EmitEventArgs e)
        {
            _emitQueue.Enqueue(new EmitRow
            {
                Time = e.Time,
                Action = e.Action,
                MachineNo = e.MachineNo,
                Device = $"{e.DeviceIP}:{e.Port}",
                EnrollmentId = e.EnrollmentId,
                MemberId = e.MemberId,
                MemberName = e.MemberName
            });
        }

        // Producer: enqueue only
        private void OnPaired(object sender, SimulationEvents.PairingEventArgs e)
        {
            _pairQueue.Enqueue(new PairRow
            {
                AttendanceId = e.AttendanceId ?? "-",
                MemberId = e.MemberId,
                MemberName = e.MemberName,
                CheckInTime = e.CheckInTime,
                CheckOutTime = e.CheckOutTime,
                Paired = e.Paired,
                Note = e.Note
            });
        }

        // Consumer: batch flush to UI-bound BindingLists
        private void FlushTimer_Tick(object sender, EventArgs e)
        {
            // Prevent reentrancy if the form is not ready
            if (!IsHandleCreated || IsDisposed) return;

            // Emit rows
            int added = 0;
            while (added < FlushBatchSize && _emitQueue.TryDequeue(out var row))
            {
                _emits.Add(row);
                added++;
            }

            // Cap size
            if (_emits.Count > MaxRows)
            {
                int toRemove = _emits.Count - MaxRows;
                for (int i = 0; i < toRemove; i++)
                {
                    _emits.RemoveAt(0);
                }
            }

            // Pair rows
            added = 0;
            while (added < FlushBatchSize && _pairQueue.TryDequeue(out var prow))
            {
                _pairs.Add(prow);
                added++;
            }

            if (_pairs.Count > MaxRows)
            {
                int toRemove = _pairs.Count - MaxRows;
                for (int i = 0; i < toRemove; i++)
                {
                    _pairs.RemoveAt(0);
                }
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (_untilUtc.HasValue)
            {
                var remaining = _untilUtc.Value - DateTime.UtcNow;
                if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
                lblRemaining.Text = $"Remaining: {remaining:hh\\:mm\\:ss}";
            }
            else
            {
                lblRemaining.Text = "Remaining: 00:00:00";
            }
        }

        private void AppendLog(string text)
        {
            // Keep log lightweight; avoid massive strings by limiting lines
            const int maxLogChars = 100_000;
            if (txtLog.TextLength > maxLogChars)
            {
                // Drop older content to keep UI snappy
                txtLog.Clear();
                txtLog.AppendText("[Log truncated]\r\n");
            }

            if (txtLog.TextLength > 0) txtLog.AppendText(Environment.NewLine);
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}");
        }

        private void UpdateUiState()
        {
            var running = _cts != null;
            btnStart.Enabled = !running;
            btnStop.Enabled = running;
            numHours.Enabled = !running;
        }

        private sealed class EmitRow
        {
            public DateTime Time { get; set; }
            public string Action { get; set; }
            public int MachineNo { get; set; }
            public string Device { get; set; }
            public int EnrollmentId { get; set; }
            public int MemberId { get; set; }
            public string MemberName { get; set; }
        }

        private sealed class PairRow
        {
            public string AttendanceId { get; set; }
            public int MemberId { get; set; }
            public string MemberName { get; set; }
            public DateTime? CheckInTime { get; set; }
            public DateTime? CheckOutTime { get; set; }
            public bool Paired { get; set; }
            public string Note { get; set; }
        }
    }
}