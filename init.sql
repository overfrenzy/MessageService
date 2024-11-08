CREATE TABLE IF NOT EXISTS messages (
    text TEXT NOT NULL,
    timestamp TIMESTAMP NOT NULL,
    sequencenumber SERIAL PRIMARY KEY
);
