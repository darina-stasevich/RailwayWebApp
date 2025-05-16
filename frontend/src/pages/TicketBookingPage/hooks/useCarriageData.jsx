import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';

export const useCarriageData = (segmentApiParams, token) => {
    const navigate = useNavigate();
    const [shortCarriagesList, setShortCarriagesList] = useState(null);
    const [isLoadingShortCarriages, setIsLoadingShortCarriages] = useState(false);
    const [shortCarriagesError, setShortCarriagesError] = useState('');

    const [detailedCarriage, setDetailedCarriage] = useState(null);
    const [isLoadingDetailedCarriage, setIsLoadingDetailedCarriage] = useState(false);
    const [detailedCarriageError, setDetailedCarriageError] = useState('');

    const fetchShortCarriages = useCallback(async () => {
        if (!segmentApiParams) return;
        setIsLoadingShortCarriages(true);
        setShortCarriagesList(null);
        setDetailedCarriage(null);
        setShortCarriagesError('');

        const requestBody = {
            concreteRouteId: segmentApiParams.concreteRouteId,
            startSegmentNumber: segmentApiParams.startSegmentNumber,
            endSegmentNumber: segmentApiParams.endSegmentNumber,
        };
        console.log("Запрос ShortCarriageInfoDto с (useCarriageData):", requestBody);

        if (!token) {
            setShortCarriagesError('Ошибка: Пользователь не авторизован.');
            setIsLoadingShortCarriages(false);
            navigate('/login');
            return;
        }
        try {
            const response = await fetch('http://localhost:5241/api/Carriages/summaries', {
                method: 'POST',
                body: JSON.stringify(requestBody),
                headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' }
            });
            if (!response.ok) throw new Error(`Ошибка загрузки данных о вагонах: ${response.statusText} (${response.status})`);
            const data = await response.json();
            setShortCarriagesList(data);
        } catch (error) {
            setShortCarriagesError(error.message);
            console.error("Ошибка загрузки ShortCarriageInfoDto (useCarriageData):", error);
        } finally {
            setIsLoadingShortCarriages(false);
        }
    }, [segmentApiParams, token, navigate]);

    const fetchDetailedCarriage = useCallback(async (carriageNumber) => {
        if (!segmentApiParams || !carriageNumber) return;
        setIsLoadingDetailedCarriage(true);
        setDetailedCarriage(null);
        setDetailedCarriageError('');

        const requestBody = {
            concreteRouteId: segmentApiParams.concreteRouteId,
            startSegmentNumber: segmentApiParams.startSegmentNumber,
            endSegmentNumber: segmentApiParams.endSegmentNumber,
            carriageNumber: carriageNumber,
        };
        console.log("Запрос DetailedCarriageInfoDto с (useCarriageData):", requestBody);

        if (!token) {
            setDetailedCarriageError('Ошибка: Пользователь не авторизован.');
            setIsLoadingDetailedCarriage(false);
            navigate('/login');
            return;
        }
        try {
            const response = await fetch('http://localhost:5241/api/Carriages/details', {
                method: 'POST',
                body: JSON.stringify(requestBody),
                headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' }
            });
            if (!response.ok) throw new Error(`Ошибка загрузки данных о вагоне: ${response.statusText} (${response.status})`);
            const data = await response.json();
            setDetailedCarriage(data);
        } catch (error) {
            setDetailedCarriageError(error.message);
            console.error("Ошибка загрузки DetailedCarriageInfoDto (useCarriageData):", error);
        } finally {
            setIsLoadingDetailedCarriage(false);
        }
    }, [segmentApiParams, token, navigate]);

    const clearDetailedCarriageView = useCallback(() => setDetailedCarriage(null), []);
    const clearShortCarriagesList = useCallback(() => setShortCarriagesList(null), []);
    const clearAllCarriageData = useCallback(() => {
        setShortCarriagesList(null);
        setDetailedCarriage(null);
        setShortCarriagesError('');
        setDetailedCarriageError('');
    }, []);


    return {
        shortCarriagesList,
        isLoadingShortCarriages,
        shortCarriagesError,
        fetchShortCarriages,
        clearShortCarriagesList,
        detailedCarriage,
        isLoadingDetailedCarriage,
        detailedCarriageError,
        fetchDetailedCarriage,
        clearDetailedCarriageView,
        clearAllCarriageData
    };
};