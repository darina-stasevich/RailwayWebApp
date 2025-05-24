import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './TicketsPage.module.css';
import TicketCard from './TicketCard/TicketCard.jsx';
import { useStations } from '../../contexts/StationsContext.jsx';
import { formatDateTime, formatDateOnly, formatGender } from '../../utils/formatters.js';

const API_BASE_URL = 'http://localhost:5241/api';

const TICKET_TYPES = {
    ACTIVE: 'active',
    CANCELLED: 'cancelled',
    EXPIRED: 'expired',
};

const TicketTypeLabels = {
    [TICKET_TYPES.ACTIVE]: 'Активные',
    [TICKET_TYPES.CANCELLED]: 'Отмененные',
    [TICKET_TYPES.EXPIRED]: 'Архив',
}

const TicketsPage = () => {
    const navigate = useNavigate();
    const { getStationNameById, isLoadingStations, errorStations } = useStations();

    const [selectedTicketType, setSelectedTicketType] = useState(TICKET_TYPES.ACTIVE);
    const [tickets, setTickets] = useState([]);
    const [isLoadingTickets, setIsLoadingTickets] = useState(false);
    const [errorTickets, setErrorTickets] = useState(null);
    const [actionMessage, setActionMessage] = useState({ type: '', text: '' });

    const fetchTickets = useCallback(async (type) => {
        setIsLoadingTickets(true);
        setErrorTickets(null);
        const token = localStorage.getItem('token');

        if (!token) {
            setErrorTickets('Пользователь не авторизован.');
            setIsLoadingTickets(false);
            navigate('/login');
            return;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/Tickets/${type}`, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                },
            });

            if (!response.ok) {
                let errorDetailMessage = `HTTP ошибка ${response.status}`;
                try {
                    const errorText = await response.text();
                    if (errorText) {
                        const errorJson = JSON.parse(errorText);
                        errorDetailMessage = errorJson.message || errorJson.title || JSON.stringify(errorJson);
                    }
                } catch (e)
                {

                }
                throw new Error(`Не удалось загрузить билеты: ${errorDetailMessage}`);
            }
            const data = await response.json();
            setTickets(data || []);
        } catch (err) {
            setErrorTickets(err.message);
            console.error(`Error fetching ${type} tickets:`, err);
        } finally {
            setIsLoadingTickets(false);
        }
    }, [navigate]);

    useEffect(() => {
        if (!isLoadingStations) {
            if (errorStations) {
                setErrorTickets(`Не удалось отобразить билеты из-за ошибки загрузки станций: ${errorStations}`);
                setTickets([]);
            } else {
                fetchTickets(selectedTicketType);
            }
        }
    }, [selectedTicketType, fetchTickets, isLoadingStations, errorStations]);

    const handleTypeChange = (type) => {
        setSelectedTicketType(type);
    };

    const handleCancelTicket = async (ticketId) => {
        if (!window.confirm('Вы уверены, что хотите отменить этот билет? Это действие необратимо.')) {
            return;
        }

        setActionMessage({ type: '', text: '' });
        setIsLoadingTickets(true);

        const token = localStorage.getItem('token');
        if (!token) {
            setActionMessage({ type: 'error', text: 'Ошибка авторизации. Пожалуйста, войдите снова.' });
            setIsLoadingTickets(false);
            navigate('/login');
            return;
        }

        try {
                const response = await fetch(`${API_BASE_URL}/Payments/cancel-pay`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(ticketId),
            });

            if (!response.ok) {
                let errorDetailMessage = `Ошибка ${response.status}`;
                let errorBodyText = '';
                try {
                   errorBodyText = await response.text();
                    if (errorBodyText) {
                        try {
                            const errorJson = JSON.parse(errorBodyText);
                            if (Array.isArray(errorJson) && response.status >= 400) {
                                errorDetailMessage = `Сервер вернул ошибку ${response.status}, но тело ответа похоже на успешные данные.`;
                            } else if (Array.isArray(errorJson)) {
                                errorDetailMessage = "Билет обработан, но ответ сервера не был ожидаемой ошибкой.";
                            }
                            else {
                                errorDetailMessage = errorJson.message || errorJson.title || JSON.stringify(errorJson);
                            }
                        } catch (jsonParseError) {
                            errorDetailMessage = errorBodyText.substring(0, 300) || errorDetailMessage;
                        }
                    }
                } catch (readError) {
                    console.error("Could not read error response body for cancel ticket:", readError);
                }
                throw new Error(`Не удалось отменить билет: ${errorDetailMessage}`);
            }

            setActionMessage({ type: 'success', text: 'Билет успешно отменен.' });
            if (selectedTicketType === TICKET_TYPES.ACTIVE) {
                fetchTickets(TICKET_TYPES.ACTIVE);
            } else {
                setIsLoadingTickets(false);
            }

        } catch (err) {
            setActionMessage({ type: 'error', text: err.message });
            setIsLoadingTickets(false);
        }

    };

    if (isLoadingStations) {
        return <div className={styles.loading}>Загрузка данных станций...</div>;
    }

    return (
        <div className={styles.container}>
            <h1>Мои билеты</h1>
            <div className={styles.ticketTypeSelector}>
                {Object.values(TICKET_TYPES).map((type) => (
                    <button
                        key={type}
                        className={`${styles.typeButton} ${selectedTicketType === type ? styles.activeType : ''}`}
                        onClick={() => handleTypeChange(type)}
                    >
                        {TicketTypeLabels[type]}
                    </button>
                ))}
            </div>

            {actionMessage.text && (
                <div className={`${styles.actionMessage} ${actionMessage.type === 'success' ? styles.successMessage : styles.errorMessage}`}>
                    {actionMessage.text}
                </div>
            )}

            {isLoadingTickets && <div className={styles.loading}>Загрузка билетов...</div>}
            {errorTickets && <div className={styles.error}>{errorTickets}</div>}

            {!isLoadingTickets && !errorTickets && tickets.length === 0 && (
                <div className={styles.noTickets}>Нет билетов для отображения в этой категории.</div>
            )}

            {!isLoadingTickets && !errorTickets && tickets.length > 0 && (
                <div className={styles.ticketsList}>
                    {tickets.map((ticket) => (
                        <TicketCard
                            key={ticket.ticketId}
                            ticket={ticket}
                            getStationNameById={getStationNameById}
                            formatDateTime={formatDateTime}
                            formatGender={formatGender}
                            formatDateOnly={formatDateOnly}
                            isCancellable={selectedTicketType === TICKET_TYPES.ACTIVE}
                            onCancelTicket={handleCancelTicket}
                        />
                    ))}
                </div>
            )}
        </div>
    );
};

export default TicketsPage;