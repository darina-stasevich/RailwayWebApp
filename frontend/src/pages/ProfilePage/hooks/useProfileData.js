import React, { useState, useCallback, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getLocalDateInputString, mapServerGenderToSelectValue } from '../../../utils/profileUtils.js';

const API_BASE_URL = 'http://localhost:5241/api';

export const useProfileData = () => {
    const navigate = useNavigate();
    const [profileData, setProfileData] = useState({
        email: '', name: '', surname: '', secondName: '',
        phoneNumber: '', birthDate: '', gender: ''
    });
    const [initialProfileData, setInitialProfileData] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [isSaving, setIsSaving] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false); // <-- НОВОЕ СОСТОЯНИЕ
    const [error, setError] = useState(null);
    const [message, setMessage] = useState({ type: '', text: '' });

    const genderOptions = [
        { value: 0, label: 'Мужской' },
        { value: 1, label: 'Женский' },
        { value: '', label: 'Не указан' }
    ];

    const fetchProfile = useCallback(async (options = {}) => {
        setIsLoading(true);
        if (!options.preserveError) setError(null);
        if (!options.preserveMessage) setMessage({ type: '', text: '' });

        const token = localStorage.getItem('token');
        if (!token) {
            navigate('/login');
            setIsLoading(false);
            return;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/UserAccounts/me`, {
                headers: { 'Authorization': `Bearer ${token}` },
            });
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Не удалось загрузить данные профиля: ${response.status} ${errorText}`);
            }
            const data = await response.json();
            const formattedData = {
                ...data,
                birthDate: getLocalDateInputString(data.birthDate),
                gender: mapServerGenderToSelectValue(data.gender)
            };
            setProfileData(formattedData);
            setInitialProfileData(JSON.parse(JSON.stringify(formattedData)));
        } catch (err) {
            setError(err.message);
            if (!options.preserveMessage || message.type !== 'success') {
                setMessage({ type: 'error', text: `Ошибка при загрузке профиля: ${err.message}` });
            }
        } finally {
            setIsLoading(false);
        }
    }, [navigate, message.type]);

    useEffect(() => {
        fetchProfile();
    }, []);

    const handleChange = useCallback((e) => {
        const { name, value } = e.target;
        if (name === "gender" && value !== "") {
            setProfileData(prev => ({ ...prev, [name]: parseInt(value, 10) }));
        } else {
            setProfileData(prev => ({ ...prev, [name]: value }));
        }
    }, []);

    const handleSubmit = useCallback(async (e) => {
        e.preventDefault();
        setIsSaving(true);
        setError(null);
        setMessage({ type: '', text: '' });
        const token = localStorage.getItem('token');

        const dataToUpdate = {
            name: profileData.name || null,
            surname: profileData.surname || null,
            secondName: profileData.secondName || null,
            phoneNumber: profileData.phoneNumber || null,
            birthDate: profileData.birthDate ? new Date(profileData.birthDate).toISOString() : null,
            gender: profileData.gender === '' ? null : profileData.gender,
        };

        try {
            const response = await fetch(`${API_BASE_URL}/UserAccounts/me`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(dataToUpdate),
            });

            if (!response.ok) {
                let detailedErrorMessage = `Ошибка ${response.status}.`;
                try {
                    const errorData = await response.json();
                    if (errorData.errors && typeof errorData.errors === 'object' && Object.keys(errorData.errors).length > 0) {
                        let specificErrorMessages = [];
                        for (const fieldName in errorData.errors) {
                            const messages = errorData.errors[fieldName];
                            if (Array.isArray(messages) && messages.length > 0) {
                                const userFriendlyFieldName = fieldName;
                                specificErrorMessages.push(`${userFriendlyFieldName}: ${messages.join('. ')}`);
                            }
                        }
                        detailedErrorMessage = specificErrorMessages.length > 0 ?
                            `${errorData.title || 'Обнаружены ошибки валидации'}. ${specificErrorMessages.join('; ')}` :
                            (errorData.title || errorData.message || JSON.stringify(errorData));
                    } else {
                        detailedErrorMessage = errorData.title || errorData.message || JSON.stringify(errorData);
                    }
                } catch (parseErr) {
                    const textError = await response.text();
                    detailedErrorMessage = textError || detailedErrorMessage;
                }
                throw new Error(detailedErrorMessage);
            }

            await fetchProfile({ preserveMessage: true, preserveError: true });
            setMessage({ type: 'success', text: 'Данные профиля успешно обновлены!' });

        } catch (err) {
            setError(err.message);
            setMessage({ type: 'error', text: err.message });
        } finally {
            setIsSaving(false);
        }
    }, [profileData, fetchProfile]);

    const handleDeleteAccount = useCallback(async () => {
        if (!confirm("Вы уверены, что хотите удалить свой аккаунт? Это действие необратимо.")) {
            return;
        }

        setIsDeleting(true);
        setError(null);
        setMessage({ type: '', text: '' });
        const token = localStorage.getItem('token');

        try {
            const response = await fetch(`${API_BASE_URL}/UserAccounts/me`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`,
                },
            });

            if (!response.ok) {
                let errorDetailMessage = `Ошибка ${response.status}. Не удалось удалить аккаунт.`;
                try {
                    const errorData = await response.json();
                    errorDetailMessage = errorData.title || errorData.message || JSON.stringify(errorData) || errorDetailMessage;
                } catch (parseErr) {
                    const textError = await response.text();
                    errorDetailMessage = textError || errorDetailMessage;
                }
                throw new Error(errorDetailMessage);
            }

            localStorage.removeItem('token');
            setMessage({ type: 'success', text: 'Аккаунт успешно удален. Вы будете перенаправлены на страницу входа.' });
            setTimeout(() => {
                navigate('/login');
            }, 2500);

        } catch (err) {
            setError(err.message);
            setMessage({ type: 'error', text: err.message });
        } finally {
            setIsDeleting(false);
        }
    }, [navigate]);

    return {
        profileData,
        initialProfileData,
        isLoading,
        isSaving,
        isDeleting,
        error,
        message,
        genderOptions,
        handleChange,
        handleSubmit,
        handleDeleteAccount
    };
};