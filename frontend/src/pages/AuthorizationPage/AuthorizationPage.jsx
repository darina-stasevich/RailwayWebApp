import styles from './AuthorizationPage.module.css';
import RegistrationFields from "./RegistrationFields.jsx";
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

const AuthorizationPage = () => {
    const [isLoginMode, setIsLoginMode] = useState(true);
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [surname, setSurname] = useState('');
    const [name, setName] = useState('');
    const [secondName, setSecondName] = useState('');
    const [phoneNumber, setPhoneNumber] = useState('');
    const [birthDate, setBirthDate] = useState('');
    const [gender, setGender] = useState('');

    const [error, setError] = useState('');
    const [successMessage, setSuccessMessage] = useState('');
    const navigate = useNavigate();

    const clearFormFields = () => {
        setEmail('');
        setPassword('');
        setSurname('');
        setName('');
        setSecondName('');
        setPhoneNumber('');
        setBirthDate('');
        setGender('');
        setError('');
        setSuccessMessage('');
    };

    const handleModeChange = () => {
        setIsLoginMode(!isLoginMode);
        clearFormFields();
    };

    const handleLoginSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccessMessage('');
        try {
            const response = await fetch('http://localhost:5241/api/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password }),
            });

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: 'Ошибка ответа сервера при входе' }));
                throw new Error(errorData.message || 'Ошибка авторизации');
            }

            const data = await response.json();
            localStorage.setItem('token', data.token);
            localStorage.setItem('role', data.role);
            localStorage.setItem('userName', data.userName);

            if (data.role === 'Admin') {
                navigate('/admin-panel');
            } else {
                navigate('/find-route');
            }
        } catch (err) {
            setError(err.message);
        }
    };

    const handleRegisterSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccessMessage('');

        if (!email || !password || !surname || !name || !birthDate) {
            setError('Пожалуйста, заполните все обязательные поля: Email, Пароль, Фамилия, Имя, Дата рождения.');
            return;
        }
        if (password.length < 4 || password.length > 30) {
            setError('Пароль должен быть от 4 до 30 символов.');
            return;
        }
        if (name.length < 2 || name.length > 40) {
            setError('Имя должно быть от 2 до 40 символов.');
            return;
        }
        if (surname.length < 2 || surname.length > 40) {
            setError('Фамилия должна быть от 2 до 40 символов.');
            return;
        }
        if (secondName && (secondName.length < 2 || secondName.length > 40)) {
            setError('Отчество (если указано) должно быть от 2 до 40 символов.');
            return;
        }
        const phoneRegex = /^\+375\s?\(?\d{2}\)?\s?\d{7}$/;
        if (phoneNumber && !phoneRegex.test(phoneNumber)) {
            setError('Некорректный формат номера телефона. Ожидается: +375(XX)XXXXXXX');
            return;
        }

        const registrationData = {
            email,
            surname,
            name,
            secondName: secondName || null,
            phoneNumber: phoneNumber || null,
            birthDate,
            gender: gender === '' ? null : parseInt(gender, 10),
            password,
        };

        try {
            const response = await fetch('http://localhost:5241/api/UserAccounts/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(registrationData),
            });

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: 'Ошибка ответа сервера при регистрации' }));
                if (errorData.errors && typeof errorData.errors === 'object') {
                    const messages = Object.values(errorData.errors).flat().join(' ');
                    throw new Error(messages || errorData.title || 'Ошибка регистрации');
                }
                throw new Error(errorData.message || 'Ошибка регистрации');
            }
            setSuccessMessage('Регистрация прошла успешно! Теперь вы можете войти.');
            setIsLoginMode(true);
            const registeredEmail = registrationData.email;
            clearFormFields();
            setEmail(registeredEmail);
        } catch (err) {
            setError(err.message);
        }
    };

    return (
        <div className={styles.container}>
            <h2 className={styles.title}>{isLoginMode ? 'Вход' : 'Регистрация'}</h2>
            <form onSubmit={isLoginMode ? handleLoginSubmit : handleRegisterSubmit}>
                <div className={styles.inputGroup}>
                    <input
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        placeholder="Email"
                        required
                        className={styles.inputField}
                    />
                </div>
                {!isLoginMode && (
                    <RegistrationFields
                            surname={surname}
                            onSurnameChange={(e) => setSurname(e.target.value)}
                            name={name}
                            onNameChange={(e) => setName(e.target.value)}
                            secondName={secondName}
                            onSecondNameChange={(e) => setSecondName(e.target.value)}
                            phoneNumber={phoneNumber}
                            onPhoneNumberChange={(e) => setPhoneNumber(e.target.value)}
                            birthDate={birthDate}
                            onBirthDateChange={(e) => setBirthDate(e.target.value)}
                            gender={gender}
                            onGenderChange={(e) => setGender(e.target.value)}
                    />
                )}
                <div className={styles.inputGroup}>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        placeholder="Пароль"
                        required
                        minLength={isLoginMode ? undefined : "4"}
                        maxLength={isLoginMode ? undefined : "30"}
                        className={styles.inputField}
                    />
                </div>
                <button
                    type="submit"
                    className={`${styles.submitButton} ${styles.button}`}
                >
                    {isLoginMode ? 'Войти' : 'Зарегистрироваться'}
                </button>
                {error && <p className={styles.errorMessage}>{error}</p>}
                {successMessage && <p className={styles.successMessage}>{successMessage}</p>}
            </form>
            <button
                onClick={handleModeChange}
                className={`${styles.toggleButton} ${styles.button}`}
            >
                {isLoginMode ? 'Нет аккаунта? Зарегистрироваться' : 'Уже есть аккаунт? Войти'}
            </button>
        </div>
    );
};

export default AuthorizationPage;