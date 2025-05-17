import React, {useState, useEffect, useCallback} from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import styles from './FinalBookingPage.module.css';
import PassengerDataCard from "./Cards/PassengerDataCard.jsx";

const API_BASE_URL = 'http://localhost:5241/api';

const FinalBookingPage = () => {
    const location = useLocation();
    const navigate = useNavigate();

    const bookingsToProcess = location.state?.bookingToProcess;
    const complexRouteDetails = location.state?.complexRouteDetails;
    const stations = location.state?.stationsList;

    const [passengerFormsData, setPassengerFormsData] = useState([]);

    const getStationNameById = useCallback((stationId) => {
        if (!stationId || !stations || stations.length === 0) return 'Неизвестная станция';
        const station = stations.find(s => String(s.id) === String(stationId));
        return station ? station.name : `ID ${stationId ? String(stationId).slice(-6) : 'N/A'}`;
    }, [stations]);

    useEffect(() => {
        if (!bookingsToProcess || bookingsToProcess.length === 0 || !complexRouteDetails || !stations) {
            if (!bookingsToProcess || bookingsToProcess.length === 0) {
                console.warn("Данные о выбранных местах (bookingsToProcess) отсутствуют или пусты. Возврат...");
            } else {
                console.warn("Необходимые данные (complexRouteDetails или stations) для страницы ввода данных пассажиров не были переданы. Возврат...");
            }
            navigate('/find-route');
            return;
        }

        const initialForms = bookingsToProcess.map(ticketSeed => {
            const routeSegmentDetails = complexRouteDetails.directRoutes.find(dr =>
                dr.concreteRouteId === ticketSeed.concreteRouteId &&
                dr.startSegmentNumber === ticketSeed.startSegmentNumber &&
                dr.endSegmentNumber === ticketSeed.endSegmentNumber
            );

            if (!routeSegmentDetails) {
                console.error("Не удалось найти детали сегмента для билета:", ticketSeed, "в complexRouteDetails:", complexRouteDetails.directRoutes);
                return null;
            }

            return {
                concreteRouteId: ticketSeed.concreteRouteId,
                startSegmentNumber: ticketSeed.startSegmentNumber,
                endSegmentNumber: ticketSeed.endSegmentNumber,
                carriageNumber: parseInt(ticketSeed.carriageNumber, 10),
                seatNumber: ticketSeed.seatNumber,
                hasBedLinenSet: ticketSeed.hasBedLinenSet || false,
                passengerData: {
                    surname: '',
                    firstName: '',
                    secondName: null,
                    gender: '',
                    birthDate: '',
                    passportNumber: ''
                },
                displayInfo: {
                    fromStationName: getStationNameById(routeSegmentDetails.fromStationId),
                    toStationName: getStationNameById(routeSegmentDetails.toStationId),
                    departureDate: routeSegmentDetails.departureDate,
                    arrivalDate: routeSegmentDetails.arrivalDate,
                    cost: routeSegmentDetails.cost
                },
                formId: `${ticketSeed.concreteRouteId}-${ticketSeed.carriageNumber}-${ticketSeed.seatNumber}`
            };
        });

        const filtered = initialForms.filter(form => form !== null);
        setPassengerFormsData(filtered);

    }, [bookingsToProcess, complexRouteDetails, stations, navigate, getStationNameById]);

    const handleInputChange = (formId, fieldPath, value) => {
        setPassengerFormsData(prevForms =>
            prevForms.map(form => {
                if (form.formId === formId) {
                    if (fieldPath.startsWith('passengerData.')) {
                        const passengerField = fieldPath.split('.')[1];
                        let processedValue = value;
                        if (passengerField === 'gender') {
                            processedValue = parseInt(value, 10);
                            if (isNaN(processedValue)) {
                                processedValue = 0;
                            }
                        }
                        return {
                            ...form,
                            passengerData: {
                                ...form.passengerData,
                                [passengerField]: processedValue
                            }
                        };
                    } else {
                        return { ...form, [fieldPath]: value };
                    }
                }
                return form;
            })
        );
    };

    const validateAllForms = () => {
        const validityStates = document.querySelectorAll('[data-form-valid]');
        return Array.from(validityStates).every(el => el.getAttribute('data-form-valid') === 'true');
    };

    const handleSubmitAllBookings = async () => {
        const allFormsValid = validateAllForms();

        if (!allFormsValid) {
            alert("Пожалуйста, исправьте ошибки в формах перед отправкой.");
            return;
        }

        console.log("Финальные данные для отправки на бэк:", passengerFormsData);

        const bookSeatRequests = passengerFormsData.map(form => ({
            concreteRouteId: form.concreteRouteId,
            startSegmentNumber: form.startSegmentNumber,
            endSegmentNumber: form.endSegmentNumber,
            carriageNumber: form.carriageNumber,
            seatNumber: form.seatNumber,
            hasBedLinenSet: form.hasBedLinenSet,
            passengerData: {
                surname: form.passengerData.surname,
                firstName: form.passengerData.firstName,
                secondName: form.passengerData.secondName || null,
                gender: form.passengerData.gender,
                birthDate: form.passengerData.birthDate,
                passportNumber: form.passengerData.passportNumber
            }
        }));

        if (!bookSeatRequests || bookSeatRequests.length === 0) {
            alert("Нет данных для бронирования.");
            return;
        }

        const token = localStorage.getItem('token');
        if (!token) {
            alert("Ошибка: Пользователь не авторизован.");
            navigate('/login');
            return;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/Books/book-seats`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(bookSeatRequests)
            });

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: "Не удалось получить детали ошибки." }));
                throw new Error(`Ошибка бронирования ${response.status}: ${errorData.title || errorData.message || response.statusText}`);
            }

            const result = await response.json();
            console.log("Ответ от сервера на бронирование:", result);
            alert("Билеты успешно забронированы!");
            navigate('/my-bookings');
        } catch (error) {
            console.error("Ошибка при отправке запроса на бронирование:", error);
            alert(`Произошла ошибка: ${error.message}`);
        }
    };

    if (passengerFormsData.length === 0) {
        return <p>Загрузка информации о выбранных местах...</p>;
    }

    return (
        <div className={styles.passengerDetailsPage}>
            <h1>Ввод данных пассажиров</h1>
            <p>Пожалуйста, заполните данные для каждого выбранного места.</p>

            {passengerFormsData.map((form, index) => (
                <PassengerDataCard
                    key={form.formId}
                    form={form}
                    index={index}
                    handleInputChange={handleInputChange}
                    formId={form.formId}
                />
            ))}

            <button
                onClick={handleSubmitAllBookings}
                className={styles.submitAllButton}
            >
                Подтвердить и забронировать все билеты
            </button>

            <p className={styles.validationMessage}>
                Пожалуйста, заполните все обязательные поля корректно перед отправкой
            </p>
        </div>
    );
}

export default FinalBookingPage;