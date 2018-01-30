#ifndef TRACKER_MANAGER_HPP
#define TRACKER_MANAGER_HPP

#include "tobii_tracker.hpp"
#include <vector>
#include <string>

class TrackerManager
{
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
