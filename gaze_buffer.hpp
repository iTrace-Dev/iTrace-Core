#ifndef GAZE_BUFFER_HPP
#define GAZE_BUFFER_HPP

#include <queue>
#include <mutex>
#include <condition_variable>
#include "gaze_data.hpp"

/*
 * Threadsafe queue used to buffer incoming gaze data
 * from a tracker.
 *
 * Data flow:
 *  Tracker -> GazeBuffer -> Output (Reticle, File, Socket, etc.)
 */
class GazeBuffer {

    public:
        static GazeBuffer* Instance();
        static void Delete ();
        GazeData* dequeue();
        void enqueue(GazeData*);

    private:
        static GazeBuffer* gbInstance;

        GazeBuffer();
        GazeBuffer(GazeData const&) {}
        GazeBuffer& operator=(GazeData const&) {}
        ~GazeBuffer() {}

        std::queue<GazeData*> buffer;
        std::mutex mutex;
        std::condition_variable cv;

};

#endif // GAZE_BUFFER_HPP
