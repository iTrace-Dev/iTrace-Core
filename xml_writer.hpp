#ifndef XML_WRITER_HPP
#define XML_WRITER_HPP

#include <QXmlStreamWriter>
#include <QFile>
#include <QObject>
#include "gaze_data.hpp"
#include "session_manager.hpp"

class XMLWriter: public QObject {
    Q_OBJECT

    public:
        explicit XMLWriter(SessionManager* sessionInfo, QObject *parent = nullptr);
        ~XMLWriter();
        void setEnvironment(const std::string& trackerID);

    public slots:
        void writeResponse(GazeData gaze);

    private:
        QXmlStreamWriter writer;
        QFile outputFile;
        SessionManager* session;
};

#endif // XML_WRITER_HPP
