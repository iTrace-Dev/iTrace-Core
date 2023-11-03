# iTrace-Core

iTrace Core is a component of the iTrace platform. The core is responsible for managing all aspects of the physical eye tracking devices. It also maintains the session state and maintains any gaze related data files produced during the study. Lastly, the core assists with plugin management indicating when to start and stop recording information in the IDE.

Presently iTrace core only supports [Tobii Pro](https://www.tobiipro.com/), 4C (with pro upgrade), [Gazepoint](https://www.gazept.com/), and [SmartEye](https://smarteye.se/) eye trackers. Support for other trackers in future releases.

### BASIC USAGE:
1) Start iTrace-Core
2) Setup a session
3) Select a tracker
4) Calibrate (if using a non-mouse tracker)
5) Connect one or more plugins
6) Start Tracking

### Code Contributors
Please read the CONTRIB.md for guidelines on iTrace-Core development
