#ifndef SERVER_HPP
#define SERVER_HPP

#include <QObject>
#include <QTcpSocket>
#include <QTcpServer>

class Server: public QObject
{
    Q_OBJECT

public:
    explicit Server(QObject *parent = 0);
    ~Server();
    void writeData();

public slots:
    void newConnection();

private:
    QTcpServer *server;
    std::vector<QTcpSocket*> clients;
    const int PORT = 8008;
};

#endif // SERVER_HPP