import { useEffect, useState } from 'react'; // React araçlarını aldık
import axios from 'axios'; // API ile konuşacak kütüphane

export default function CarList()
{
    const [cars,setCars]=useState([]);
    const [loading,setLoading]=useState(true);
    
    useEffect(() => {
        // Backend'e istek atıyoruz
        axios.get("http://localhost:5295/api/Cars") 
            .then((response) => {
                // Başarılıysa veriyi kutuya koy
                console.log("Veri Geldi:", response.data); // Konsola da yazdıralım
                setCars(response.data);
                setLoading(false);
            })
            .catch((error)=>{
                console.error("Hata oluştu:",error);
                setLoading(false);
            })
        },[]);

        if(loading)
        {
            return 
            <div className="text-center mt-5">
                <h5>Veriler yükleniyor...</h5>
            </div>
        }

        return(
            <div className="card shadow-sm">
                <div className="car-header bg-primary text-white">
                    <h4 className="mb-0">Araç Listesi ({cars.length} Adet)</h4>                    
                </div>
                <div className="card-body">
                    <table className="table table-hover table-striped">
                    <thead>
                        <tr>
                            <th>Marka / Model</th>
                            <th>Paket</th>
                            <th>Yıl</th>
                            <th>Yakıt</th>
                            <th>Vites</th>
                            <th>Beygir</th>
                        </tr>
                    </thead>
                    <tbody>
                        {cars.map((car) => (
                            <tr key={car.id}>
                                <td>{car.brandModel}</td>
                                <td>{car.package}</td>
                                <td><span className="badge bg-secondary">{car.year}</span></td>
                                <td>{car.fuel}</td>
                                <td>{car.gear}</td>
                                <td>{car.horsePower} HP</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                </div>
            </div>
            
        )


    
}