#ifndef GAZE_HANDLER_HPP
#define GAZE_HANDLER_HPP

#include <QObject>
#include <QThreadPool>
#include <QXmlStreamWriter>
#include <string>
#include "gaze_buffer.hpp"
#include "server.hpp"

/*
 * Worker thread that removes gaze data from the gaze buffer
 * and coordinates with sources that need the data (output,
 * ui elements, socket, etc).
 *
 * IMPORTANT NOTE:
 * The worker thread stops when it encounters gaze_data that
 * has the value nullptr. Tracker manager shuts down this thread
 * after stopping the tracker by enqueuing this value to the
 * gaze_buffer. This ensures the queue is clean of data in between
 * starting and stopping the tracker and simplifies thread syncronization.
 */
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
