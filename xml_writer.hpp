#ifndef XML_WRITER_HPP
#define XML_WRITER_HPP

#include <QXmlStreamWriter>
#include <QFile>
#include <QObject>

class XMLWriter: public QObject {
    Q_OBJECT

    public:
        explicit XMLWriter(QObject *parent = nullptr);
        ~XMLWriter();
        void setEnvironment(const std::string& trackerID);

    private:
        QXmlStreamWriter writer;
        QFile outputFile;
};

#endif // XML_WRITER_HPP
