import React, { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import AiService from '../services/aiService';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import './AiRecommendationPage.css';

const aiService = new AiService();

const INITIAL_MESSAGE = {
    sender: 'bot',
    text: 'Merhaba! Ben yapay zeka destekli araç danışmanınızım. 🚗\nSiz hayalinizdeki özellikleri söyleyin, ben veritabanımızdaki en uygun araçları sizin için bulayım.'
};

export default function AiRecommendationPage() {
    const [messages, setMessages] = useState(() => {
        const savedMessages = sessionStorage.getItem("ai_chat_messages");
        if (savedMessages) {
            try { return JSON.parse(savedMessages); }
            catch (e) { return [INITIAL_MESSAGE]; }
        }
        return [INITIAL_MESSAGE];
    });

    const [input, setInput] = useState("");
    const [loading, setLoading] = useState(false);
    const messagesEndRef = useRef(null);
    const navigate = useNavigate();

    useEffect(() => {
        sessionStorage.setItem("ai_chat_messages", JSON.stringify(messages));
    }, [messages]);

    const scrollToBottom = () => messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    useEffect(() => { scrollToBottom(); }, [messages]);

    const getImageUrl = (url) => {
        if (!url || url === "NotFound" || url === "undefined") {
            return "https://www.bmw-m.com/content/dam/bmw/marketBMW_M/www_bmw-m_com/topics/magazine-article-pool/2021/e46-gtr-street/bmw-m3-gtr-street-stage-teaser.jpg";
        }
        if (url.startsWith("http")) return url;
        return `/car_images/${url}`;
    };

    const handleSend = async (textToSend) => {
        const messageText = typeof textToSend === 'string' ? textToSend : input;
        if (!messageText.trim() || loading) return;
        setMessages(prev => [...prev, { sender: 'user', text: messageText }]);
        setInput("");
        setLoading(true);
        try {
            const result = await aiService.ask(messageText);
            setMessages(prev => [...prev, { sender: 'bot', text: result.data.answer }]);
        } catch (error) {
            console.error("AI Hata:", error.response?.data || error.message);
            setMessages(prev => [...prev, { sender: 'bot', text: 'Sistemsel bir bağlantı sorunu oluştu. Lütfen tekrar deneyin.' }]);
        } finally {
            setLoading(false);
        }
    };

    const handleReset = async () => {
        await aiService.clearSession().catch(() => { });
        setMessages([INITIAL_MESSAGE]);
        sessionStorage.removeItem("ai_chat_messages");
        setInput("");
    };

    // Ortak ReactMarkdown bileşen ayarları (inline style ile CSS'e bağımsız)
    const mdComponents = {
        blockquote: ({ children }) => <>{children}</>,
        p: ({ children }) => <p style={{ margin: 0, padding: 0 }}>{children}</p>,
        img: ({ node, ...props }) => (
            <img
                src={getImageUrl(props.src)}
                alt={props.alt}
                referrerPolicy="no-referrer"
                style={{
                    width: '100%', height: '185px', objectFit: 'contain',
                    display: 'block', backgroundColor: '#f8fafc',
                    padding: '10px', borderBottom: '1px solid #edf2f7',
                    boxSizing: 'border-box', borderRadius: '14px 14px 0 0',
                }}
            />
        ),
        h3: ({ children }) => (
            <h3 style={{
                fontSize: '1rem', fontWeight: 800, margin: 0,
                padding: '12px 14px 6px', color: '#1a202c',
                whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis',
            }}>{children}</h3>
        ),
        ul: ({ children }) => (
            <ul style={{
                listStyle: 'none', margin: 0, padding: '4px 12px 10px',
                display: 'flex', flexWrap: 'wrap', gap: '6px', flex: 1,
            }}>{children}</ul>
        ),
        li: ({ children }) => (
            <li style={{
                background: '#f1f5f9', padding: '4px 10px', borderRadius: '8px',
                fontSize: '0.78rem', color: '#334155', fontWeight: 600,
                border: '1px solid #e2e8f0', display: 'inline-flex', alignItems: 'center',
            }}>{children}</li>
        ),
        a: ({ node, ...props }) => (
            <div style={{ padding: '0 14px 14px', marginTop: 'auto' }}>
                <span
                    onClick={() => navigate(props.href)}
                    style={{
                        display: 'block', textAlign: 'center', cursor: 'pointer',
                        background: 'linear-gradient(135deg,#2d3748,#1a202c)',
                        color: '#fff', padding: '10px', borderRadius: '8px',
                        fontWeight: 700, fontSize: '0.85rem',
                    }}
                >
                    {props.children} ↗
                </span>
            </div>
        ),
    };

    /**
     * Satır satır parse: '> ' ile başlayan satır gruplarını kart bloğu olarak ayırır.
     * AI'ın giriş metni ile kart arasına tek veya çift newline koyması fark etmez.
     */
    const renderBotMessage = (text) => {
        if (!text.includes('> ![')) {
            return <ReactMarkdown remarkPlugins={[remarkGfm]} components={mdComponents}>{text}</ReactMarkdown>;
        }

        const lines = text.split('\n');
        const introLines = [];
        const carBlocks = [];
        let currentBlock = null;

        for (const line of lines) {
            if (line.startsWith('> ') || line === '>') {
                // Blockquote satırı — mevcut bloğa ekle
                if (!currentBlock) currentBlock = [];
                currentBlock.push(line);
            } else {
                // Blockquote dışı satır (boş satır dahil)
                if (currentBlock) {
                    // Bloğu kapat
                    const isCarBlock = currentBlock.some(l => l.startsWith('> !['));
                    if (isCarBlock) {
                        carBlocks.push(currentBlock.join('\n'));
                    } else {
                        // Araç kartı değil — intro'ya aktar (> prefix'i sil)
                        introLines.push(...currentBlock.map(l => l.replace(/^>\s?/, '')));
                    }
                    currentBlock = null;
                }
                if (line.trim()) introLines.push(line);
            }
        }
        // Son bloğu kapat
        if (currentBlock) {
            const isCarBlock = currentBlock.some(l => l.startsWith('> !['));
            if (isCarBlock) carBlocks.push(currentBlock.join('\n'));
            else introLines.push(...currentBlock.map(l => l.replace(/^>\s?/, '')));
        }

        if (carBlocks.length === 0) {
            return <ReactMarkdown remarkPlugins={[remarkGfm]} components={mdComponents}>{text}</ReactMarkdown>;
        }

        const introText = introLines.join('\n').trim();

        return (
            <>
                {introText && (
                    <div style={{ width: '100%', marginBottom: '14px' }}>
                        <ReactMarkdown remarkPlugins={[remarkGfm]} components={mdComponents}>{introText}</ReactMarkdown>
                    </div>
                )}
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '14px', width: '100%' }}>
                    {carBlocks.map((carMd, i) => (
                        <div key={i} style={{
                            background: '#fff', borderRadius: '14px', overflow: 'hidden',
                            boxShadow: '0 6px 20px rgba(0,0,0,0.08)',
                            display: 'flex', flexDirection: 'column',
                            transition: 'transform 0.2s, box-shadow 0.2s',
                        }}>
                            <ReactMarkdown remarkPlugins={[remarkGfm]} components={mdComponents}>{carMd}</ReactMarkdown>
                        </div>
                    ))}
                </div>
            </>
        );
    };

    return (
        <div className="premium-ai-wrapper">
            <div className="ai-header-premium" style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '12px' }}>
                <h1 className="ai-title-premium">✨ AI Araç Danışmanı</h1>
                {messages.length > 1 && (
                    <button
                        onClick={handleReset}
                        style={{ fontSize: '12px', padding: '4px 10px', cursor: 'pointer', opacity: 0.6, border: '1px solid #ccc', borderRadius: '4px', background: 'transparent' }}
                        title="Sohbeti sıfırla"
                    >
                        🔄 Sıfırla
                    </button>
                )}
            </div>

            <div className="ai-messages-area">
                {messages.map((msg, index) => (
                    <div key={index} className={`ai-message-row ${msg.sender}`}>
                        <div className="ai-message-content-wrapper">
                            <div className="ai-avatar">
                                {msg.sender === 'bot' ? '🤖' : '👤'}
                            </div>
                            <div className="ai-bubble">
                                {msg.sender === 'user'
                                    ? msg.text
                                    : renderBotMessage(msg.text)
                                }
                            </div>
                        </div>
                    </div>
                ))}

                {loading && (
                    <div className="ai-message-row bot">
                        <div className="ai-message-content-wrapper">
                            <div className="ai-avatar">🤖</div>
                            <div className="ai-bubble loading-bubble">Düşünüyor...</div>
                        </div>
                    </div>
                )}
                <div ref={messagesEndRef} />
            </div>

            {messages.length < 3 && !loading && (
                <div className="quick-prompts-premium">
                    <button onClick={() => handleSend("Az yakan dizel sedan araç öner")}>⛽ Az Yakan Sedan</button>
                    <button onClick={() => handleSend("Geniş bagajlı aile arabası var mı?")}>👨‍👩‍👧‍👦 Aile Arabası</button>
                    <button onClick={() => handleSend("Başlangıç için ucuz ve küçük araç öner")}>🎓 Öğrenci İşi</button>
                </div>
            )}

            <div className="ai-input-area-premium">
                <div className="input-container-centered">
                    <input
                        type="text"
                        placeholder="Örn: 2015 model üstü, otomatik, sedan..."
                        value={input}
                        onChange={(e) => setInput(e.target.value)}
                        onKeyPress={(e) => e.key === 'Enter' && handleSend()}
                        disabled={loading}
                    />
                    <button onClick={() => handleSend()} disabled={loading}>Gönder</button>
                </div>
            </div>
        </div>
    );
}