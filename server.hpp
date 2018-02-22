#ifndef SERVER_HPP
#define SERVER_HPP

#include <QObject>
#include <QTcpSocket>
#include <QTcpServer>
#include <string>

/*
 * The server is strictly used for communication between
 * the Core and any plugins. Data is sent as a string.
 */
class Server: public QObject {
    Q_OBJECT

    public:
        explicit Server(QObject *parent = nullptr);
        ~Server();

    public slots:
        void newConnection();
        void clientDisconnect();
        void writeData(std::string data);

    private:
        QTcpServer *server;
        std::vector<QTcpSocket*> clients;
        const int PORT = 8008;
};

#endif // SERVER_HPP
