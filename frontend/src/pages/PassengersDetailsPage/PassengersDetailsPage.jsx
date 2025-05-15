import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { formatDateTime, formatDuration } from '../../utils/formatters.js';
import styles from './PassengersDetailsPage.module.css';

const PassengersDetailsPage = () => {
    const location = useLocation();
    const navigate = useNavigate();

    const bookingsToProcess = location.state?.bookingsToProcess;
    const complexRouteDetails = location.state?.complexRouteDetails;
    const stations = location.state?.stations;

      const [passengerFormsData, setPassengerFormsData] = useState([]);

    const getStationNameById = (stationId) => {
        if (!stationId || !stations || stations.length === 0) return 'Неизвестная станция';
        const station = stations.find(s => String(s.id) === String(stationId));
        return station ? station.name : `ID ${stationId.slice(-6)}`;
    };

    useEffect(() => {
        if (!bookingsToProcess || !complexRouteDetails || !stations) {
            console.warn("Необходимые данные для страницы ввода данных пассажиров не были переданы. Возврат...");
            navigate('/find-route');
            return;
        }

        const initialFormsData = [];
        bookingsToProcess.forEach((segmentBooking, segmentIndex) => {
            const routeSegmentDetails = complexRouteDetails.directRoutes[segmentIndex];
            if (!routeSegmentDetails) return;

            segmentBooking.selectionsByCarriage.forEach((selectedSeatsSet, carriageNumber) => {
                selectedSeatsSet.forEach(seatNumber => {
                    initialFormsData.push({
                        concreteRouteId: segmentBooking.concreteRouteId,
                        startSegmentNumber: segmentBooking.startSegmentNumber,
                        endSegmentNumber: segmentBooking.endSegmentNumber,
                        carriageNumber: parseInt(carriageNumber, 10),
                        seatNumber: seatNumber,
                        hasBedLinenSet: false,
                        passengerData: {
                            surname: '',
                            firstName: '',
                            secondName: '',
                            gender: 0,
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
                        formId: `${segmentBooking.concreteRouteId}-${carriageNumber}-${seatNumber}`
                    });
                });
            });
        });
        setPassengerFormsData(initialFormsData);

    }, [bookingsToProcess, complexRouteDetails, stations, navigate]);

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

    const handleSubmitAllBookings = async () => {
        console.log("Финальные данные для отправки на бэк:", passengerFormsData);

        const bookSeatRequests = passengerFormsData.map(form => ({
            concreteRouteId: form.concreteRouteId,
            startSegmentNumber: form.startSegmentNumber,
            endSegmentNumber: form.endSegmentNumber,
            carriageNumber: form.carriageNumber,
            seatNumber: form.seatNumber,
            hasBedLinenSet: form.hasBedLinenSet,
            passengerData: {
                ...form.passengerData,

            }
        }));

        console.log("Сформированный List<BookSeatRequest>:", bookSeatRequests);


        console.log("Сформированный List<BookSeatRequest> ПЕРЕД stringify:", JSON.stringify(bookSeatRequests, null, 2)); // <--- ОЧЕНЬ ВАЖНО

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
            const response = await fetch('http://localhost:5241/api/Books/book-seats', {
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
            // navigate('/my-bookings');
            navigate('/find-route');


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
                <div key={form.formId} className={styles.passengerFormCard}>
                    <h3>Билет {index + 1}</h3>
                    <p>
                        Маршрут: <strong>{form.displayInfo.fromStationName} → {form.displayInfo.toStationName}</strong>
                    </p>
                    <p>
                        Отправление: {formatDateTime(form.displayInfo.departureDate)},
                        Прибытие: {formatDateTime(form.displayInfo.arrivalDate)}
                    </p>
                    <p>
                        Вагон: <strong>{form.carriageNumber}</strong>, Место: <strong>{form.seatNumber}</strong>
                    </p>

                    <h4>Данные пассажира:</h4>
                    <div className={styles.formGrid}>
                        <label>Фамилия:
                            <input type="text" value={form.passengerData.surname} onChange={(e) => handleInputChange(form.formId, 'passengerData.surname', e.target.value)} required />
                        </label>
                        <label>Имя:
                            <input type="text" value={form.passengerData.firstName} onChange={(e) => handleInputChange(form.formId, 'passengerData.firstName', e.target.value)} required />
                        </label>
                        <label>Отчество (если есть):
                            <input type="text" value={form.passengerData.secondName || ''} onChange={(e) => handleInputChange(form.formId, 'passengerData.secondName', e.target.value)} />
                        </label>
                        <label>Пол:
                            <select
                                value={String(form.passengerData.gender)}
                                onChange={(e) => handleInputChange(form.formId, 'passengerData.gender', e.target.value)}
                                required
                            >
                                <option value="">Выберите пол</option>
                                <option value="0">Мужской</option>
                                <option value="1">Женский</option>
                            </select>
                        </label>
                        <label>Дата рождения:
                            <input type="date" value={form.passengerData.birthDate} onChange={(e) => handleInputChange(form.formId, 'passengerData.birthDate', e.target.value)} required />
                        </label>
                        <label>Номер паспорта (серия и номер):
                            <input type="text" value={form.passengerData.passportNumber} onChange={(e) => handleInputChange(form.formId, 'passengerData.passportNumber', e.target.value)} required />
                        </label>
                    </div>

                    <label className={styles.bedLinenLabel}>
                        <input
                            type="checkbox"
                            checked={form.hasBedLinenSet}
                            onChange={(e) => handleInputChange(form.formId, 'hasBedLinenSet', e.target.checked)}
                        />
                        Включить постельное белье
                    </label>
                </div>
            ))}

            <button onClick={handleSubmitAllBookings} className={styles.submitAllButton}>
                Подтвердить и забронировать все билеты
            </button>
        </div>
    );

}

export default PassengersDetailsPage;
