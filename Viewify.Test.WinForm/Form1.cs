using Viewify.Core.Render;

namespace Viewify.Test.WinForm;

public partial class Form1 : Form
{

    Scheduler _scheduler;
    WinFormsNativeHandler _handler;
    TestView _rootNode;
    int _testValue = 0;
    bool _isRendering = false;

    public Form1()
    {

        InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        _rootNode = new(_testValue);
        _handler = new(flowLayoutPanel1);
        _scheduler = new(_rootNode, _handler);
        //_isRendering = true;
        TriggerRender();
    }

    private void TriggerRender()
    {
        if (!_isRendering || _handler == null || _scheduler == null)
        {
            return;
        }
        _scheduler.Tick();
        SetTimeout(100, TriggerRender);
    }

    private System.Windows.Forms.Timer timer = new();
    private void SetTimeout(int milliseconds, Action callback)
    {
        timer.Tick += (sender, e) =>
        {
            timer.Stop();
            callback();
            timer.Interval = milliseconds;
            timer.Start();
        };
        timer.Interval = milliseconds;
        timer.Start();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        ++_testValue;
        _rootNode.IntState %= _testValue;
    }

    private void button2_Click(object sender, EventArgs e)
    {
        _scheduler.Tick();
    }
}