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

        emit socketOut(gd->toString());
        emit websocketOut(gd->toString());
        emit reticleOut(int(gd->leftX), int(gd->leftY));
        emit xmlOut(*gd);
        delete gd;
        gd = GazeBuffer::Instance().dequeue();
    }
}
