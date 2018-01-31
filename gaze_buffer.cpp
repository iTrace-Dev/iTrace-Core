#include "gaze_buffer.hpp"

GazeBuffer* GazeBuffer::gbInstance = nullptr;

GazeBuffer* GazeBuffer::Instance(QObject* parent) {
    if (!gbInstance)
        gbInstance = new GazeBuffer(parent);

    return gbInstance;
}

GazeBuffer::GazeBuffer(QObject* parent): QObject(parent), buffer() {}

GazeBuffer::~GazeBuffer() {
    std::unique_lock<std::mutex> mlock(mutex);
    while(!buffer.empty()) {
        delete buffer.front();
        buffer.pop();
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

