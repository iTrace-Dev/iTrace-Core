#include "gaze_handler.hpp"
#include "gaze_buffer.hpp"
#include <QDebug>

GazeHandler::GazeHandler(int displayWidth, int displayHeight): screenWidth(displayWidth), screenHeight(displayHeight) {}

void GazeHandler::run() {
    GazeData* gd = GazeBuffer::Instance().dequeue();

    while (gd) {
        //Update gaze coordinates for display
        gd->leftX *= screenWidth;
        gd->leftY *= screenHeight;
        gd->rightX *= screenWidth;
        gd->rightY *= screenHeight;

        emit socketOut(gd->toString());
        emit websocketOut(gd->toString());
        emit reticleOut(int(gd->leftX), int(gd->leftY));
        emit xmlOut(*gd);
        delete gd;
        gd = GazeBuffer::Instance().dequeue();
    }
}
