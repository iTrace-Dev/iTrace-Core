#include "gaze_writer.hpp"
#include "gaze_data.hpp"
#include <QDebug>

GazeWriter::GazeWriter() {
    buffer = GazeBuffer::Instance();
    server = nullptr;
}

GazeWriter::GazeWriter(Server* serv): GazeWriter() {
    server = serv;
}

void GazeWriter::run() {
    GazeData* gd = buffer->dequeue();

    while (gd) {
        //server->writeData(std::to_string(gd->leftX) + ", " + std::to_string(gd->leftY));
        qDebug() << gd->leftX << " " << gd->leftY;
        delete gd;
        gd = buffer->dequeue();
    }
}
