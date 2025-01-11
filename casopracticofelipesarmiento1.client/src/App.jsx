import { useEffect, useState } from 'react';
import './App.css';

function App() {
    const [corn, setCorn] = useState();
    const [message, setMessage] = useState();
    const [clientId, setClientId] = useState()

    const buyCorn = async () => {
        try {
            const response = await fetch('Corn/purchase', {
                method: 'POST',
                headers: {
                    'ClientId': clientId
                }
            });

            if (response.status === 200) {
                const data = await response.text();
                setMessage(data); // Mensaje exitoso desde el backend
            } else if (response.status === 429) {
                setMessage('Too Many Requests: Limit exceeded.');
            } else {
                setMessage('An unexpected error occurred: ' + response.status);
            }
        } catch (error) {
            setMessage('An error occurred: ' + error.message + " " + error.toString());
        }
    };

    return (
        <div className="flex flex-col gap-8">
            <h1 id="tableLabel" className="text-white font-bold text-center text-5xl">Bob's Corn</h1>
            <input type="text" onChange={ ({target}) => {
                setClientId(target.value)
            } }/>
            <button onClick={ () => {
                setMessage("");
                buyCorn()
            } } className="border rounded-lg text-white p-3 font-bold border-2 border-white hover:border-green">🌽 Buy 1 Corn 🌽</button>
            <p className="text-yellow-300 text-center font-bold">You can buy 1 corn per minute</p>
            <p className="text-green-300 text-center font-bold">{message}</p>
        </div>
    );
}

export default App;