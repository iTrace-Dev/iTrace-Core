#ifndef GAZE_WRITER_HPP
#define GAZE_WRITER_HPP

#include <QObject>
#include <QThreadPool>
#include <QXmlStreamWriter>
#include <string>
#include "gaze_buffer.hpp"
#include "server.hpp"

class GazeWriter : public QObject, public QRunnable {
    Q_OBJECT

    public:
        GazeWriter();
        ~GazeWriter() {}
        void run();

    signals:
        void socketOut(std::string);
        void reticleOut(double x, double y);

    private:
        GazeBuffer* buffer;
};

#endif // GAZE_WRITER_HPP
