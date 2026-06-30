import React, { useState, useRef, useEffect } from 'react';
import AiService from '../services/aiService';
import './ChatWidget.css'; // Birazdan oluşturacağız

export default function ChatWidget() {
    const [isOpen, setIsOpen] = useState(false);
    const [messages, setMessages] = useState([
        { sender: 'bot', text: 'Merhaba! Ben CarAdvisor asistanıyım. Sana nasıl yardımcı olabilirim? (Örn: Az yakan otomatik araç öner)' }
    ]);
    const [input, setInput] = useState("");
    const [loading, setLoading] = useState(false);
    
    // Mesaj gelince otomatik aşağı kaydırma için
    const messagesEndRef = useRef(null);

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    };

    useEffect(() => {
        scrollToBottom();
    }, [messages]);

    const handleSend = async () => {
        if (!input.trim()) return;

        // 1. Kullanıcı mesajını ekle
        const userMsg = { sender: 'user', text: input };
        setMessages(prev => [...prev, userMsg]);
        setInput("");
        setLoading(true);

        try {
            // 2. API'ye sor
            let aiService = new AiService();
            const result = await aiService.ask(input);
            
            // 3. Bot cevabını ekle
            // Backend'den { answer: "..." } dönüyor
            const botMsg = { sender: 'bot', text: result.data.answer };
            setMessages(prev => [...prev, botMsg]);
        } catch (error) {
            console.error(error);
            setMessages(prev => [...prev, { sender: 'bot', text: 'Üzgünüm, şu an bağlantı kuramıyorum.' }]);
        } finally {
            setLoading(false);
        }
    };

    // Enter tuşuna basınca göndermek için
    const handleKeyPress = (e) => {
        if (e.key === 'Enter') handleSend();
    };

    return (
        <div className="chat-widget-wrapper">
            {isOpen && (
                <div className="chat-window">
                    <div className="chat-header">
                        <span>🤖 CarAdvisor AI</span>
                        <button className="close-btn" onClick={() => setIsOpen(false)}>×</button>
                    </div>
                    
                    <div className="chat-messages">
                        {messages.map((msg, index) => (
                            <div key={index} className={`message ${msg.sender}`}>
                                <div className="message-bubble">{msg.text}</div>
                            </div>
                        ))}
                        {loading && <div className="message bot"><div className="message-bubble">Yazıyor...</div></div>}
                        <div ref={messagesEndRef} />
                    </div>

                    <div className="chat-input-area">
                        <input 
                            type="text" 
                            placeholder="Bir şeyler sor..." 
                            value={input}
                            onChange={(e) => setInput(e.target.value)}
                            onKeyPress={handleKeyPress}
                        />
                        <button onClick={handleSend} disabled={loading}>➤</button>
                    </div>
                </div>
            )}

            {/* 2. Yuvarlak Açma Butonu */}
            <button className={`chat-toggle-btn ${isOpen ? 'hide' : ''}`} onClick={() => setIsOpen(true)}>
                💬
            </button>
        </div>
    );
}