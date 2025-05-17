import React, {createContext, useState, useEffect, useContext, useCallback} from 'react';
import {useNavigate} from "react-router-dom";

const API_BASE_URL = 'http://localhost:5241/api';
const StationsContext = createContext();

export const useStations = () => {
    const context = useContext(StationsContext);
    if (context === undefined) {
        throw new Error('useStations must be used within a StationsProvider');
    }
    return context;
};

export const StationsProvider = ({ children }) => {
    const [stations, setStations] = useState([]);
    const [isLoadingStations, setIsLoadingStations] = useState(true);
    const [errorStations, setErrorStations] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        const loadStations = async () => {
            setIsLoadingStations(true);
            setErrorStations(null);

            const token = localStorage.getItem('token');

            const headers = {
                'Content-Type': 'application/json',
            };

            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            } else {
                navigate('/login');
            }

            try {
                const response = await fetch(`${API_BASE_URL}/Stations`, {
                    method: 'GET',
                    headers: headers,
                });

                if (!response.ok) {
                    let errorDetailMessage = `HTTP ошибка ${response.status}`;
                    try {
                        const errorText = await response.text();
                        if (errorText) {
                            try {
                                const errorJson = JSON.parse(errorText);
                                errorDetailMessage = errorJson.message || errorJson.title || JSON.stringify(errorJson);
                            } catch (jsonParseError) {
                                errorDetailMessage = errorText.substring(0, 200) || errorDetailMessage;
                            }
                        }
                    } catch (e) {
                        console.error("StationsProvider: Could not read error response body", e);
                    }
                    throw new Error(`Не удалось загрузить станции: ${errorDetailMessage}`);
                }

                const data = await response.json();
                setStations(data || []);
            } catch (err) {
                if (err.message.includes("Пользователь не авторизован")) {
                    setErrorStations(err.message);
                } else {
                    setErrorStations(err.message || "Произошла неизвестная ошибка при загрузке станций.");
                }
                console.error("Error in StationsProvider loading stations:", err);
            } finally {
                setIsLoadingStations(false);
            }
        };

        loadStations();
    }, []);

    const getStationNameById = useCallback((stationId) => {
        if (isLoadingStations) return 'Загрузка станций...';
        if (errorStations) return 'Ошибка загрузки станций';
        if (!stations || stations.length === 0) return 'Список станций пуст';
        if (!stationId) return 'Станция не указана';

        const station = stations.find(s => String(s.id) === String(stationId));
        return station ? station.name : `ID ${String(stationId).slice(-6)}`;
    }, [stations, isLoadingStations, errorStations]);

    return (
        <StationsContext.Provider value={{ stations, isLoadingStations, errorStations, getStationNameById }}>
            {children}
        </StationsContext.Provider>
    );
};