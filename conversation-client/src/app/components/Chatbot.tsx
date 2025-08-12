'use client';

import React, { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

export default function Chatbot() {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [messages, setMessages] = useState<{ user: string; message: string }[]>([]);
    const [input, setInput] = useState('');
    const user = "User"
    
    useEffect(() => {
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${process.env.NEXT_PUBLIC_API_URL}/chatHub`)
            .withAutomaticReconnect()
            .build();

        newConnection.start()
            .then(() => {
                console.log('SignalR connected');
                setConnection(newConnection);

                newConnection.on('ReceiveMessage', (user: string, message: string) => {
                    setMessages((prev) => [...prev, { user, message }]);
                });
            })
            .catch((err) => console.error('SignalR connection error:', err));

        return () => {
            newConnection.stop();
        };
    }, []);

    const sendMessage = async () => {
        if (connection && input.trim()) {
            try {
                await connection.invoke('SendMessage', user, input);
                setInput('');
            } catch (err) {
                console.error('Error sending message:', err);
            }
        }
    };

    return (
        <div className="max-w-lg mx-auto p-4 bg-gray-100 rounded-lg shadow-md">
            <h1 className="text-2xl font-bold text-center mb-4">Chat with Chatbot</h1>
            <div className="h-64 overflow-y-auto bg-white p-4 rounded-lg border border-gray-300 mb-4">
                {messages.map((msg, index) => (
                    <div
                        key={index}
                        className={`mb-2 ${msg.user === 'Chatbot' ? 'text-blue-600 text-right' : 'text-gray-800 text-left'
                            }`}
                    >
                        <strong>{msg.user}:</strong> {msg.message}
                    </div>
                ))}
            </div>
            <div className="flex items-center gap-2">
                <input
                    type="text"
                    className="flex-1 p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="Type your message..."
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                />
                <button
                    className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500"
                    onClick={sendMessage}
                >
                    Send
                </button>
            </div>
        </div>
    );
};