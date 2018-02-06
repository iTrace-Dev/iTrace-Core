#include "server.hpp"
#include "gaze_buffer.hpp"
#include <QDebug>

Server::Server(QObject *parent): QObject(parent) {
    server = new QTcpServer(this);
    connect(server, SIGNAL(newConnection()), this, SLOT(newConnection()));
    if (!server->listen(QHostAddress::Any, PORT)) {
        qDebug() << "Server could not start";
    }
}

Server::~Server() {
    for (std::vector<QTcpSocket*>::const_iterator it = clients.begin(); it != clients.end(); ++it) {
        (*it)->close();
    }
    server->close();
}

void Server::newConnection() {
    qDebug() << "CLIENT CONNECTED!";
    clients.push_back(server->nextPendingConnection());
}

void Server::writeData(std::string value) {
    for (std::vector<QTcpSocket*>::const_iterator it = clients.begin(); it != clients.end(); ++it) {
        (*it)->write(value.c_str());
        (*it)->flush();
        (*it)->waitForBytesWritten();
    }
}
