#include "gaze_buffer.hpp"

GazeBuffer& GazeBuffer::Instance() {
    static GazeBuffer singleton;
    return singleton;
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
