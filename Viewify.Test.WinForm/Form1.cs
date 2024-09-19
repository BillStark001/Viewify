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

        Application.Idle += Application_Idle;

        _isRendering = true;
    }

    private long lastTick;
    private long minTickInterval = 100000;
    private long maxTickExecutionTime = 80000;

    private void Application_Idle(object? sender, EventArgs e)
    {
        if (_isRendering && !IsDisposed && !Disposing)
        {
            var now = DateTime.UtcNow.Ticks;
            if (now - lastTick >= minTickInterval)
            {
                _scheduler.Tick();
                lastTick = now;
            }
        }
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