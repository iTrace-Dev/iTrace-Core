#include "gaze_writer.hpp"
#include "gaze_data.hpp"
#include <QDebug>

GazeWriter::GazeWriter() {
    buffer = GazeBuffer::Instance();
}

void GazeWriter::run() {
    GazeData* gd = buffer->dequeue();

    while (gd) {
        emit socketOut(gd->toString());
        //server->writeData(std::to_string(gd->leftX) + ", " + std::to_string(gd->leftY));
        //qDebug() << gd->leftX << " " << gd->leftY;
        delete gd;
        gd = buffer->dequeue();
    }
}
