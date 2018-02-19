#include "gaze_buffer.hpp"
#include <QDebug>

GazeBuffer* GazeBuffer::gbInstance = nullptr;

GazeBuffer* GazeBuffer::Instance() {
    if (!gbInstance)
        gbInstance = new GazeBuffer();

    return gbInstance;
}

GazeBuffer::GazeBuffer(): buffer() {}

void GazeBuffer::Delete () {
    if (gbInstance) {
        delete gbInstance;
        gbInstance = nullptr;
    }
}

void GazeBuffer::enqueue(GazeData* gd) {
    std::unique_lock<std::mutex> mlock(mutex);
    buffer.push(gd);
    mlock.unlock();
    cv.notify_one();
}

GazeData* GazeBuffer::dequeue() {
    std::unique_lock<std::mutex> mlock(mutex);
    while(buffer.empty()) {
        cv.wait(mlock);
    }
    GazeData* gd = buffer.front();
    buffer.pop();
    return gd;
}
