#include "gaze_handler.hpp"
#include "gaze_buffer.hpp"
#include "session_manager.hpp"
#include <QDebug>

void GazeHandler::run() {
    GazeData* gd = GazeBuffer::Instance().dequeue();
    SessionManager& session = SessionManager::Instance();

    while (gd) {
        //Update gaze coordinates for display (does not apply to mouse)
        if (gd->trackerType != "mouse") {
            gd->leftX *= session.getScreenWidth();
            gd->leftY *= session.getScreenHeight();
            gd->rightX *= session.getScreenWidth();
            gd->rightY *= session.getScreenHeight();
        }

        std::string gazeString = createPluginData(gd->eventTime, gd->getCalculatedX(), gd->getCalculatedY());

        emit socketOut(gazeString);
        emit websocketOut(gazeString);
        emit reticleOut(int(gd->leftX), int(gd->leftY));
        emit xmlOut(*gd);
        emit eyeStatusOut(*gd);
        delete gd;
        gd = GazeBuffer::Instance().dequeue();
    }
}

std::string GazeHandler::createPluginData(int64_t eventID, double x, double y) {
    return std::string("gaze") + ',' + std::to_string(eventID) + ',' + std::to_string(x) + ',' + std::to_string(y) + '\n';
}
