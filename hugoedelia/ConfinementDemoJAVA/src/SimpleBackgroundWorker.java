import java.awt.EventQueue;


public abstract class SimpleBackgroundWorker {
	
	public abstract Object doInBackground() throws Exception;
	
	public abstract void done();
	
	private final Object _lock;
	private Object _result;
	private boolean _failed;
	
	public SimpleBackgroundWorker()
	{
		_result = null;
		_lock = new Object();
	}
	
	public final void execute()
	{
		new Thread() {
			public void run()
			{
				try {
					Object result = doInBackground();
					synchronized(_result)
					{
						_result = result;
						_failed = false;
					}
					EventQueue.invokeAndWait(new Runnable() {
						public void run() { done(); }
					});
				}
				catch(Exception e)
				{
					synchronized(_lock)
					{
						_result = e;
						_failed = true;
					}
				}
			}
		}.start();
	}
	
	public final Object get() throws Exception
	{
		synchronized(_lock)
		{
			if(_failed)
				throw (Exception) _result;
			return _result;
		}
	}
}
