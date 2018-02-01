#include <iterator>
#include <QDebug>
#include "tracker_manager.hpp"
#include "mouse_tracker.hpp"

TrackerManager::TrackerManager():activeTracker(nullptr), tobiiTrackers(nullptr) {

    Tracker* m = new MouseTracker();
    availableTrackers.push_back(m);

    tobiiTrackers = get_tobii_trackers();

    if (tobiiTrackers) {
        for (size_t i = 0; i < tobiiTrackers->count; ++i) {
            qDebug() << "Found a tracker";
            availableTrackers.push_back(new TobiiTracker(tobiiTrackers->eyetrackers[i]));
        }
    }
}

TrackerManager::~TrackerManager() {
    for (std::vector<Tracker*>::const_iterator it = availableTrackers.begin(); it != availableTrackers.end(); ++it) {
        if (*it != nullptr) {
            delete(*it);
        }
    }
    if (tobiiTrackers != nullptr) {
        tobii_research_free_eyetrackers(tobiiTrackers);
    }
}

std::vector<std::string> TrackerManager::getTrackerNames() {
    std::vector<std::string> trackerNames;

    for (std::vector<Tracker*>::const_iterator it = availableTrackers.begin(); it != availableTrackers.end(); ++it)
        trackerNames.push_back((*it)->trackerName());

    return trackerNames;
}

void TrackerManager::setActiveTracker(const std::string& name) {
    for (std::vector<Tracker*>::const_iterator it = availableTrackers.begin(); it != availableTrackers.end(); ++it) {
        if (name == (*it)->trackerName()) {
            activeTracker = (*it);
            break;
        }
    }
}

Tracker* TrackerManager::getActiveTracker() {
    return activeTracker;
}

void TrackerManager::startTracking() {
    activeTracker->startTracker();
}

void TrackerManager::stopTracking() {
    activeTracker->stopTracker();
}
