const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');

const app = express();
const PORT = 3000;

// Middleware
app.use(cors());
app.use(bodyParser.urlencoded({ extended: true }));
app.use(bodyParser.json());

// In-memory store
const loggedInUsers = new Set();
const waitingPlayers = [];

// ---- LOGIN ENDPOINT ----
app.post('/login', (req, res) => {
    const username = req.body.username;

    if (!username || typeof username !== 'string') {
        return res.status(400).json({ message: 'Invalid username' });
    }

    if (loggedInUsers.has(username)) {
        return res.status(409).json({ message: 'Username already taken' });
    }

    loggedInUsers.add(username);
    console.log(`User logged in: ${username}`);
    res.json({ success: true, username });
});

// ---- MATCHMAKING ENDPOINT ----
const matchedPlayers = new Set();

app.post('/matchmake', (req, res) => {
    const username = req.body.username;

    if (!username || typeof username !== 'string') {
        return res.status(400).json({ message: 'Invalid username' });
    }

    // If already matched, return client role
    if (matchedPlayers.has(username)) {
        return res.json({
            success: true,
            host: "127.0.0.1",
            port: 7777,
            role: "client"
        });
    }

    // Try to match with a waiting player
    for (let i = 0; i < waitingPlayers.length; i++) {
        const other = waitingPlayers[i];
        if (other !== username && !matchedPlayers.has(other)) {
            waitingPlayers.splice(i, 1);
            matchedPlayers.add(username);
            matchedPlayers.add(other);

            console.log(`Matched ${other} with ${username}`);

            return res.json({
                success: true,
                host: "127.0.0.1",
                port: 7777,
                role: "client"
            });
        }
    }

    if (!waitingPlayers.includes(username) && !matchedPlayers.has(username)) 
    {
        waitingPlayers.push(username);
        console.log(`${username} is waiting for opponent...`);
    }

    res.json({
        success: true,
        role: "waiting",
        message: "Waiting for opponent..."
    });
});

// ---- CANCEL MATCHMAKING ENDPOINT ----
app.post('/cancel', (req, res) => {
    const username = req.body.username;

    if (!username || typeof username !== 'string') {
        return res.status(400).json({ message: 'Invalid username' });
    }

    let removedFromQueue = false;
    const index = waitingPlayers.indexOf(username);
    if (index !== -1) {
        waitingPlayers.splice(index, 1);
        removedFromQueue = true;
        console.log(`${username} cancelled matchmaking and was removed from queue.`);
    }

    if (matchedPlayers.has(username)) {
        matchedPlayers.delete(username);
        console.log(`User ${username} was removed from matched list.`);
    }

    if (loggedInUsers.has(username)) {
        loggedInUsers.delete(username);
        console.log(`User ${username} was logged out.`);
    }

    res.json({
        success: true,
        message: `${username} was ${removedFromQueue ? 'removed from queue and' : ''} logged out.`
    });
});

// ---- START SERVER ----
app.listen(PORT, () => {
    console.log(`Matchmaking server running at http://localhost:${PORT}`);
});