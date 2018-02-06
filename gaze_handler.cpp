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
        //server->writeData(std::to_string(gd->leftX) + ", " + std::to_string(gd->leftY));
        //qDebug() << gd->leftX << " " << gd->leftY;
        delete gd;
        gd = buffer->dequeue();
    }
}
