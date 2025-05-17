import styles from "./MessageDisplay.module.css";
import React from "react";

const MessageDisplay = ({message, error}) => {
    return (
        <>
            {message.text && (
                <div
                    className={`${styles.message} ${message.type === 'success' ? styles.successMessage : styles.errorMessage}`}>
                    {message.text}
                </div>
            )}
            {error && (!message.text || message.type !== 'error') && <div className={styles.error}>{error}</div>}
        </>
    );
}

export default MessageDisplay;