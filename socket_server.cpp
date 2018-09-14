#include "socket_server.hpp"
#include <QDebug>

SocketServer::SocketServer(QObject *parent): QObject(parent) {
    server = new QTcpServer(this);
    connect(server, SIGNAL(newConnection()), this, SLOT(newConnection()));
    if (!server->listen(QHostAddress::Any, PORT)) {
        qDebug() << "Server could not start";
    }
}

SocketServer::~SocketServer() {
    //Proper Socket Clean-up
    for (std::vector<QTcpSocket*>::const_iterator it = clients.begin(); it != clients.end(); ++it) {
        (*it)->close();
        (*it)->deleteLater();
    }
    clients.clear();
    server->close();
    server->deleteLater();
}

void SocketServer::newConnection() {
    qDebug() << "SOCKET CLIENT CONNECTED!";
    QTcpSocket* client_conn = server->nextPendingConnection();
    clients.push_back(client_conn);
}

size_t SocketServer::clientCount() {
    return clients.size();
}

void SocketServer::clientCleanup() {
    std::vector<QTcpSocket*>::const_iterator it = clients.begin();
    while (it != clients.end()) {
        if ((*it)->state() == QAbstractSocket::UnconnectedState) {
            (*it)->close();
            (*it)->deleteLater();
            it = clients.erase(it);
            qDebug() << "Client Cleaned Up!";
        }
        else {
            ++it;
        }
    }
}

void SocketServer::writeData(std::string value) {
    std::vector<QTcpSocket*>::const_iterator it = clients.begin();
    while (it != clients.end()) {
        if ((*it)->state() == QAbstractSocket::UnconnectedState) {
            (*it)->close();
            (*it)->deleteLater();
            it = clients.erase(it);
            qDebug() << "Remove Disconnected Client!";
        }
        else{
            (*it)->write(value.c_str());
            (*it)->flush();
            (*it)->waitForBytesWritten();
            ++it;
        }
    }
}
