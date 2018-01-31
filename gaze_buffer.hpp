#ifndef GAZE_BUFFER_HPP
#define GAZE_BUFFER_HPP

#include <QObject>
#include <queue>
#include <mutex>
#include <condition_variable>
#include "gaze_data.hpp"

class GazeBuffer: public QObject
{
    Q_OBJECT

public:
    static GazeBuffer* Instance( QObject* parent = nullptr);
    GazeData* dequeue();
    void enqueue(GazeData*);
    ~GazeBuffer();

private:
    static GazeBuffer* gbInstance;

    GazeBuffer(QObject* parent);
    GazeBuffer(GazeData const&) {}
    GazeBuffer& operator=(GazeData const&) {}

    std::queue<GazeData*> buffer;
    std::mutex mutex;
    std::condition_variable cv;

};

#endif // GAZE_BUFFER_HPP
