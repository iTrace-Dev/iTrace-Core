#ifndef GAZE_WRITER_HPP
#define GAZE_WRITER_HPP

#include <QThreadPool>
#include <QXmlStreamWriter>
#include "gaze_buffer.hpp"
#include "server.hpp"

class GazeWriter : public QRunnable {

    public:
        GazeWriter();
        GazeWriter(Server*);
        ~GazeWriter() {}
        void run();

    private:
        GazeBuffer* buffer;
        Server* server;
};

#endif // GAZE_WRITER_HPP
