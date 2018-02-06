#ifndef GAZE_HANDLER_HPP
#define GAZE_HANDLER_HPP

#include <QObject>
#include <QThreadPool>
#include <QXmlStreamWriter>
#include <string>
#include "gaze_buffer.hpp"
#include "server.hpp"

class GazeHandler : public QObject, public QRunnable {
    Q_OBJECT

    public:
        GazeHandler();
        ~GazeHandler() {}
        void run();

    signals:
        void socketOut(std::string);
        void reticleOut(double x, double y);

    private:
        GazeBuffer* buffer;
};

#endif // GAZE_HANDLER_HPP
