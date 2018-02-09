#include "gaze_handler.hpp"
#include "gaze_data.hpp"
#include <QDebug>

GazeHandler::GazeHandler() {
    buffer = GazeBuffer::Instance();
}

void GazeHandler::run() {
    GazeData* gd = buffer->dequeue();

    while (gd) {
        emit socketOut(gd->toString());
        emit reticleOut(gd->leftX, gd->leftY);
        emit xmlOut(*gd);
        delete gd;
        gd = buffer->dequeue();
    }
}
