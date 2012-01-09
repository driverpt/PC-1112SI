import java.awt.*;
import java.awt.event.*;
import java.util.concurrent.ExecutionException;

import javax.swing.*;


public final class ConfinementDemoForm extends JFrame {

	private final JLabel message = new JLabel();
	private final JButton doIt = new JButton("Do It!"); 
	
	public ConfinementDemoForm()
	{
		
		setLayout();
		setBehaviour();
	}
	
	private void setBehaviour() 
	{
		doIt.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent evt)
			{
				doIt.setEnabled(false);
				message.setText("Doing it...");
				new SwingWorker<String, Object>() {
					
					public String doInBackground()
					{
						// take your time
						try { 
							Thread.sleep(10000);
						}
						catch(InterruptedException ie) { }
						return "Done!!!";
					}
					
					public void done()
					{
						String result = "";
						try { result = get(); } catch (Exception e) { }
						message.setText(result);
						doIt.setEnabled(true);
					}
				}.execute();
			}
		});
	}

	private void setLayout() 
	{
		this.add(message, BorderLayout.CENTER);
		JPanel panel = new JPanel();
		panel.add(doIt);
		this.add(panel, BorderLayout.SOUTH);
		setSize(200, 100);
	}

	/**
	 * @param args
	 */
	public static void main(String[] args) 
	{
		JFrame frame = new ConfinementDemoForm();
		frame.setVisible(true);
		System.out.println("Main ending...");
	}
}
