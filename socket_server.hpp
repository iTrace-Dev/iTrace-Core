#ifndef SOCKET_SERVER_HPP
#define SOCKET_SERVER_HPP

#include <QObject>
#include <QTcpSocket>
#include <QTcpServer>
#include <vector>
#include <string>

/*
 * The server is strictly used for communication between
 * the Core and any plugins. Data is sent as a string.
 */
class SocketServer: public QObject {
    Q_OBJECT

    public:
        explicit SocketServer(QObject *parent = nullptr);
        ~SocketServer();
        size_t clientCount();
        void clientCleanup();

    public slots:
        void newConnection();
        void writeData(std::string data);

    private:
        QTcpServer *server;
        std::vector<QTcpSocket*> clients;
        const int PORT = 8008;
};

#endif // SOCKET_SERVER_HPP
