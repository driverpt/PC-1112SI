package org.isel.deetc.leic.pc;

public class PhasedGate {
  private final Object _monitor = new Object();

  private int          _maxParticipants;
  private int          _currentParticipants;
  private boolean      _isSignaled;
  private boolean      _isUsed;

  public PhasedGate(int participants) {
    _maxParticipants = participants;
    _currentParticipants = 0;
    _isSignaled = false;
  }

  public void await() throws InterruptedException {
    synchronized (_monitor) {
      if (_isUsed) {
        throw new IllegalStateException();
      }
      if (++_currentParticipants == _maxParticipants) {
        _isSignaled = true;
        _isUsed = true;
        _monitor.notifyAll();
      }
      while (true) {
        if (_isSignaled) {
          return;
        }
        try {
          _monitor.wait();
        } catch (InterruptedException e) {
          --_currentParticipants;
          throw e;
        }

      }
    }
  }

  public void removeParticipant() {
    synchronized (_monitor) {
      if (_isUsed) {
        throw new IllegalStateException();
      }
      if (_currentParticipants == --_maxParticipants) {
        _isSignaled = true;
        _isUsed = true;
        _monitor.notifyAll();
      }
    }
  }
}
