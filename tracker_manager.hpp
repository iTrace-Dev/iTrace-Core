#ifndef TRACKER_MANAGER_HPP
#define TRACKER_MANAGER_HPP

#include "tobii_tracker.hpp"
#include <vector>
#include <string>

/*
 * Coordinates the collection of available
 * trackers and the active tracker (as determined
 * by the user). All general tracker actions are
 * handled by this class.
 */
class TrackerManager {

    public:
        TrackerManager();
        ~TrackerManager();

        std::vector<std::string> getTrackerNames();
        void setActiveTracker(const std::string&);
        Tracker* getActiveTracker();
        void startTracking();
        void stopTracking();

    private:
        Tracker* activeTracker;
        std::vector<Tracker*> availableTrackers;

        // TOBII Eyetracker collection
        TobiiResearchEyeTrackers*  tobiiTrackers;
};

#endif // TRACKER_MANAGER_HPP
