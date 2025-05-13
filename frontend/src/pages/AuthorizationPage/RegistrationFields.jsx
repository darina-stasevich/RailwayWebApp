import React from 'react';
import styles from './AuthorizationPage.module.css';

const RegistrationFields = ({surname, onSurnameChange,
                             name, onNameChange,
                             secondName, onSecondNameChange,
                             phoneNumber, onPhoneNumberChange,
                             birthDate, onBirthDateChange,
                             gender, onGenderChange }) =>
{
    return (
        <>
            <div className={styles.inputGroup}>
                <label htmlFor="surname" className={styles.label}>Фамилия:</label>
                <input
                    type="text"
                    id="surname"
                    value={surname}
                    onChange={onSurnameChange}
                    placeholder="Фамилия"
                    required
                    minLength="2"
                    maxLength="40"
                    className={styles.inputField}
                />
            </div>

            <div className={styles.inputGroup}>
                <label htmlFor="name" className={styles.label}>Имя:</label>
                <input
                    type="text"
                    id="name"
                    value={name}
                    onChange={onNameChange}
                    placeholder="Имя"
                    required
                    minLength="2"
                    maxLength="40"
                    className={styles.inputField}
                />
            </div>

            <div className={styles.inputGroup}>
                <label htmlFor="secondName" className={styles.label}>Отчество (необязательно):</label>
                <input
                    type="text"
                    id="secondName"
                    value={secondName}
                    onChange={onSecondNameChange}
                    placeholder="Отчество (необязательно)"
                    minLength="2"
                    maxLength="40"
                    className={styles.inputField}
                />
            </div>

            <div className={styles.inputGroup}>
                <label htmlFor="phoneNumber" className={styles.label}>Номер телефона:</label>
                <input
                    type="tel"
                    id="phoneNumber"
                    value={phoneNumber}
                    onChange={onPhoneNumberChange}
                    placeholder="Номер телефона (+375XXYYYYYYY)"
                    pattern="^\+375\s?\(?\d{2}\)?\s?\d{7}$"
                    title="Формат: +375(XX)XXXXXXX или +375 XX XXXXXXX"
                    className={styles.inputField}
                />
            </div>

            <div className={styles.inputGroup}>
                <label htmlFor="birthDate" className={styles.label}>Дата рождения:</label>
                <input
                    type="date"
                    id="birthDate"
                    value={birthDate}
                    onChange={onBirthDateChange}
                    required
                    className={styles.inputField}
                />
            </div>

            <div className={styles.inputGroup}>
                <label htmlFor="gender" className={styles.label}>Пол (необязательно):</label>
                <select
                    id="gender"
                    value={gender}
                    onChange={onGenderChange}
                    className={styles.selectField}
                >
                    <option value="">Не выбрано</option>
                    <option value="0">Мужской</option>
                    <option value="1">Женский</option>
                </select>
            </div>
        </>
    );
};

export default RegistrationFields;