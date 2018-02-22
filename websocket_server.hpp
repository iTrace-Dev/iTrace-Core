#ifndef WEBSOCKET_SERVER_HPP
#define WEBSOCKET_SERVER_HPP

#include <QObject>
#include <QtWebSockets/QWebSocketServer>
#include <QtWebSockets/QtWebSockets>
#include <vector>
#include <string>

/*
 * The server is strictly used for communication between
 * the Core and any browser based plugins. Data is sent as a string.
 */
class WebsocketServer: public QObject {
    Q_OBJECT

    public:
        explicit WebsocketServer(QObject *parent = nullptr);
        ~WebsocketServer();

    public slots:
        void newConnection();
        void writeData(std::string data);

    private:
        QWebSocketServer *server;
        std::vector<QWebSocket*> clients;
        const int PORT = 7007;
};

#endif // WEBSOCKET_SERVER_HPP
