import axios from "axios";

const API_URL = "http://localhost:5295/api/Ai/ask";

// SESSION_ID'yi her seferinde yeniden üretmek yerine hafızadan okuyoruz
const getSessionId = () => {
    let sid = sessionStorage.getItem("ai_session_id");
    if (!sid) {
        sid = `session_${Date.now()}`;
        sessionStorage.setItem("ai_session_id", sid);
    }
    return sid;
};

export default class AiService {
    async ask(question) {
        return axios.post(API_URL, 
            { userQuestion: question, sessionId: getSessionId() }
        );
    }

    clearSession() {
        sessionStorage.removeItem("ai_session_id"); // Hafızayı da temizle
        return axios.delete(`http://localhost:5295/api/Ai/session/${getSessionId()}`);
    }
}