#ifndef XML_WRITER_HPP
#define XML_WRITER_HPP

#include <QXmlStreamWriter>
#include <QFile>
#include <QObject>
#include "gaze_data.hpp"

class XMLWriter: public QObject {
    Q_OBJECT

    public:
        explicit XMLWriter(QObject *parent = nullptr);
        ~XMLWriter();
        void setEnvironment(const std::string& trackerID);

    public slots:
        void writeResponse(GazeData gaze);

    private:
        QXmlStreamWriter writer;
        QFile outputFile;
};

#endif // XML_WRITER_HPP
