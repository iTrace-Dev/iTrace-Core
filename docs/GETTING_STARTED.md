### Getting Started with iTrace Core

Before staring using iTrace Core and associated plugins, it is important to ensure that your environment is configured properly. iTrace Core uses the monitor designated as "main" or "primary" by the Windows operating system for eye positions and screen dimensions. Use the Windows display settings window to verify that your eye tracker is beneath the display set to be "main" or "primary". Additionally, communication between iTrace Core and plugins relies on socket communication over port 8080. If other applications or devices are also running on that port this will interfere with iTrace communcation.

### General Usage

Launch iTrace Core ensuring that all eye-trackers are connected to the computer and the respective device drivers have been installed. 

When the iTrace Core interface loads, select the tracker you wish to use from the drop down menu of connected devices. Once a tracker has been selected, use the button `Session Setup` to configure information about the data recording session and destination directory for data storage. To help organize the data collection process, iTrace Core will use the data directory provided as a root directory to create a folder structure in the following manner:

`/Data Directory/Study Name/Participant ID`

Researcher id along with study name and participant id are stored in the output files as well (discussed in output format).

Once the session settings have been configured, the tracker can then be calibrated using the `Calibrate` button. Note that if calibration appears to "hang" this is due to the fact that the eye tracker is unable to get an acceptable reading from the user at the given point. Once the tracker can aquire a data point, calibration will resume. When Calibration is complete a screen will show all data points captured during calibration to show accuracy represented by blue and red dots for left and right eyes respectively. To exit the calibration results screen simply left click the mouse in the calibration window. The calibration data will be stored in xml format in the same folder structure described above in a directory called `calibration`. Each calibration attempt will produce a new xml file named using a standard unix style timestamp.

After calibration, iTrace plugins (supporting Visual Studio 2017 and Eclipse) can be connected to the core.

To begin a recording session press the `Start Tracker` button and the core will begin recording gaze data from the eye tracker.

When finishing a session, press `Stop Tracker` on the core. A stopped session can be resumed by pressing `Start Tracker` again.

When completely finished with a session, remember to stop the tracker using the core and disconnect the plugin.

### Data Output

Each recording session performed by iTrace Core will be recorded to a folder named using a standard unix style timestamp. These folders will be contained within the session structure `/Data Directory/Study Name/Participant ID`. Each folder will contain at least one xml file with data recorded by the core. Subsequent files will be created by each plugin.

### Data Format

Data produced by the core is in the XML format with the following structure:

``` xml
<core>
    <environment>
        <screen-size width="" height=""/>
        <eye-tracker type=""/>
        <date></date>
        <time></time>
        <session id=""/>
        <calibration id=""/>
        <study name=""/>
        <researcher name=""/>
        <participant id=""/>
    </environment>
    <response x="" y="" left_x="" left_y="" left_pupil_diameter="" left_validation="" right_x="" right_y="" right_pupil_diameter="" right_validation="" tracker_time="" system_time="" event_time="" fixation_id=""/>
</core>
```

The environment subtree of the xml document includes all metadata information about the current recording session.
Response tags all include all the data points recorded by the eye tracker with the exception of system_time, event_time, and fixation_id which are used in post processing phases.
