#include "gaze_handler.hpp"
#include "gaze_buffer.hpp"
#include <QDebug>

GazeHandler::GazeHandler() {}

void GazeHandler::run() {
    GazeData* gd = GazeBuffer::Instance().dequeue();

    while (gd) {
        emit socketOut(gd->toString());
        emit websocketOut(gd->toString());
        emit reticleOut(int(gd->leftX), int(gd->leftY));
        emit xmlOut(*gd);
        delete gd;
        gd = GazeBuffer::Instance().dequeue();
    }
}
