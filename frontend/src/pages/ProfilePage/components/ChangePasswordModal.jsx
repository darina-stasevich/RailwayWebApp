import React, { useState } from 'react';
import buttonStyles from '../../../styles/ButtonStyles.module.css'
import styles from './ChangePasswordModal.module.css';

const ChangePasswordModal = ({ isOpen, onClose, onSubmit, isChangingPassword }) => {
    const [oldPassword, setOldPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [duplicateNewPassword, setDuplicateNewPassword] = useState('');
    const [error, setError] = useState('');
    const [successMessage, setSuccessMessage] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccessMessage('');

        if (newPassword !== duplicateNewPassword) {
            setError('Пожалуйста, подтвердите новый пароль.');
            return;
        }
        if (newPassword.length < 4) {
            setError('Новый пароль должен содержать не менее 4 символов.');
            return;
        }

        const result = await onSubmit({ oldPassword, newPassword, duplicateNewPassword});

        if (result && result.success) {
            setSuccessMessage(result.message || 'Пароль успешно изменен!');
            setOldPassword('');
            setNewPassword('');
            setDuplicateNewPassword('');
        } else if (result && result.error) {
            setError(result.error);
        }
    };

    if (!isOpen) {
        return null;
    }

    return (
        <div className={styles.modalOverlay}>
            <div className={styles.modalContent}>
                <h2>Изменить пароль</h2>
                <form onSubmit={handleSubmit}>
                    {error && <div className={styles.modalError}>{error}</div>}
                    {successMessage && <div className={styles.modalSuccess}>{successMessage}</div>}

                    <div className={styles.formGroup}>
                        <label htmlFor="oldPassword">Старый пароль:</label>
                        <input
                            type="password"
                            id="oldPassword"
                            value={oldPassword}
                            onChange={(e) => setOldPassword(e.target.value)}
                            required
                        />
                    </div>
                    <div className={styles.formGroup}>
                        <label htmlFor="newPassword">Новый пароль:</label>
                        <input
                            type="password"
                            id="newPassword"
                            value={newPassword}
                            onChange={(e) => setNewPassword(e.target.value)}
                            required
                        />
                    </div>
                    <div className={styles.formGroup}>
                        <label htmlFor="duplicateNewPassword">Подтвердите новый пароль:</label>
                        <input
                            type="password"
                            id="duplicateNewPassword"
                            value={duplicateNewPassword}
                            onChange={(e) => setDuplicateNewPassword(e.target.value)}
                            required
                        />
                    </div>
                    <div className={styles.modalActions}>
                        <button type="submit" className={buttonStyles.confirmButton} disabled={isChangingPassword}>
                            {isChangingPassword ? 'Сохранение...' : 'Сохранить'}
                        </button>
                        <button type="button" className={buttonStyles.cancelButton} onClick={onClose} disabled={isChangingPassword}>
                            Отмена
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default ChangePasswordModal;